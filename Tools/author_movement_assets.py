"""One-time native asset authoring fallback; run only before Unity imports this phase.

Uses Unity YAML and an installed sprite importer metadata template. Does not access
or write the original project. Refuses to replace an existing scene or prefab.
"""
from pathlib import Path
import hashlib, json, re, struct, zlib

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / 'Assets/EchoFall'
SCENE = BASE / 'Scenes/MovementLab.unity'
PREFAB = BASE / 'Prefabs/Player.prefab'
if SCENE.exists() or PREFAB.exists():
    raise SystemExit('Preserving existing authored scene/prefab.')

def guid(path):
    p = Path(path)
    meta = Path(str(p) + '.meta')
    if meta.exists():
        found = re.search(r'^guid: (\w+)', meta.read_text(), re.M)
        if found: return found[1]
    return hashlib.md5(p.relative_to(ROOT).as_posix().encode()).hexdigest()

def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding='utf-8', newline='\n')

def meta(path, importer='DefaultImporter', fields=''):
    write(Path(str(path)+'.meta'), f'fileFormatVersion: 2\nguid: {guid(path)}\n{importer}:\n  externalObjects: {{}}\n{fields}  userData: \n  assetBundleName: \n  assetBundleVariant: \n')

def ref(path, fid=11400000):
    return f'{{fileID: {fid}, guid: {guid(path)}, type: 3}}'

HEADER = '%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
COMMON = '  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'
def section(kind, fid, name, fields):
    return f'--- !u!{kind} &{fid}\n{name}:\n{COMMON}{fields}'

def script(fid, go, name, fields=''):
    return section(114, fid, 'MonoBehaviour', f'  m_GameObject: {{fileID: {go}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {ref(BASE / ("Scripts/"+name+".cs"), 11500000)}\n  m_Name: \n  m_EditorClassIdentifier: \n'+fields)

def gameobject(fid, name, components, layer=0, tag='Untagged'):
    return section(1,fid,'GameObject','  serializedVersion: 6\n  m_Component:\n'+''.join(f'  - component: {{fileID: {c}}}\n' for c in components)+f'  m_Layer: {layer}\n  m_Name: {json.dumps(name)}\n  m_TagString: {tag}\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n')

def transform(fid, go, pos=(0,0,0), scale=(1,1,1), parent=0, children=()):
    children_yaml = '\n'+''.join(f'  - {{fileID: {c}}}\n' for c in children) if children else ' []\n'
    return section(4,fid,'Transform',f'  m_GameObject: {{fileID: {go}}}\n  serializedVersion: 2\n  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}\n  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}\n  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}\n  m_ConstrainProportionsScale: 0\n  m_Children:{children_yaml}  m_Father: {{fileID: {parent}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}\n')

def collider(fid,go,size,offset=(0,0),trigger=False):
    return section(61,fid,'BoxCollider2D',f'  m_GameObject: {{fileID: {go}}}\n  m_Enabled: 1\n  serializedVersion: 3\n  m_Density: 1\n  m_Material: {{fileID: 0}}\n  m_IsTrigger: {int(trigger)}\n  m_UsedByEffector: 0\n  m_CompositeOperation: 0\n  m_Offset: {{x: {offset[0]}, y: {offset[1]}}}\n  m_Size: {{x: {size[0]}, y: {size[1]}}}\n  m_EdgeRadius: 0\n')

# Assign stable script/assembly metadata before any references are written.
for p in BASE.rglob('*'):
    if p.suffix == '.cs': meta(p,'MonoImporter','  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n')
    elif p.suffix == '.asmdef': meta(p,'AssemblyDefinitionImporter')

# Sprite atlas metadata, with top-origin frame order and the original foot pivot.
template = next((ROOT/'Library/PackageCache').glob('com.unity.2d.common@*/Samples~/SpriteAtlas/BuiltIn/Sprites/Gem_Atlas.png.meta')).read_text()
atlas = BASE/'Art/Wanderer/wanderer-smooth.png'
header = template.split('  spriteSheet:')[0]
header = re.sub(r'guid: \w+', 'guid: '+guid(atlas), header, count=1)
idtable = ''.join(f'  - first:\n      213: {21300000+i}\n    second: Wanderer_{i:02}\n' for i in range(40))
header = re.sub(r'  internalIDToNameTable:.*?  externalObjects:', '  internalIDToNameTable:\n'+idtable+'  externalObjects:', header, flags=re.S)
header = header.split('  platformSettings:')[0] + '  platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 2048\n    textureFormat: -1\n    textureCompression: 0\n    compressionQuality: 100\n    overridden: 0\n'
sprites=''
for i in range(40):
    spriteid=hashlib.md5(f'wanderer-{i}'.encode()).hexdigest()
    sprites+=f'    - serializedVersion: 2\n      name: Wanderer_{i:02}\n      rect:\n        serializedVersion: 2\n        x: {i%4*192}\n        y: {(9-i//4)*192}\n        width: 192\n        height: 192\n      alignment: 9\n      pivot: {{x: 0.5, y: {1-161.529/192}}}\n      border: {{x: 0, y: 0, z: 0, w: 0}}\n      customData: \n      outline: []\n      physicsShape: []\n      tessellationDetail: -1\n      bones: []\n      spriteID: {spriteid}\n      internalID: {21300000+i}\n      vertices: []\n      indices: \n      edges: []\n      weights: []\n'
names=''.join(f'      Wanderer_{i:02}: {21300000+i}\n' for i in range(40))
write(Path(str(atlas)+'.meta'),header+'  spriteSheet:\n    serializedVersion: 2\n    sprites:\n'+sprites+'    outline: []\n    customData: \n    physicsShape: []\n    bones: []\n    spriteID: \n    internalID: 0\n    vertices: []\n    indices: \n    edges: []\n    weights: []\n    secondaryTextures: []\n    nameFileIdTable:\n'+names+'  userData: Original Echo-Fall atlas, 100 PPU with 0.34 visual scale\n  assetBundleName: \n  assetBundleVariant: \n')

# A small white primitive sprite; no external art dependencies.
block=BASE/'Art/Block.png'
def chunk(tag,data): return struct.pack('>I',len(data))+tag+data+struct.pack('>I',zlib.crc32(tag+data)&0xffffffff)
block.write_bytes(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',4,4,8,6,0,0,0))+chunk(b'IDAT',zlib.compress((b'\0'+b'\xff'*16)*4))+chunk(b'IEND',b''))
blockmeta=header.replace(guid(atlas),guid(block))
blockmeta=re.sub(r'  internalIDToNameTable:.*?  externalObjects:', '  internalIDToNameTable: []\n  externalObjects:',blockmeta,flags=re.S)
blockmeta=blockmeta.replace('spriteMode: 2','spriteMode: 1').replace('spritePixelsToUnits: 100','spritePixelsToUnits: 4')
write(Path(str(block)+'.meta'),blockmeta+'  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    secondaryTextures: []\n    nameFileIdTable: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
shader=next((ROOT/'Library/PackageCache').glob('com.unity.render-pipelines.universal@*/Shaders/2D/Sprite-Unlit-Default.shader'))
material=BASE/'Materials/MovementUnlit.mat'
write(material,HEADER+section(21,2100000,'Material',f'  serializedVersion: 8\n  m_Name: MovementUnlit\n  m_Shader: {ref(shader,4800000)}\n  m_Parent: {{fileID: 0}}\n  m_ValidKeywords: []\n  m_InvalidKeywords: []\n  m_LightmapFlags: 4\n  m_EnableInstancingVariants: 0\n  m_DoubleSidedGI: 0\n  m_CustomRenderQueue: -1\n  stringTagMap: {{}}\n  disabledShaderPasses: []\n  m_SavedProperties:\n    serializedVersion: 3\n    m_TexEnvs: []\n    m_Ints: []\n    m_Floats: []\n    m_Colors: []\n'))
meta(material,'NativeFormatImporter','  mainObjectFileID: 2100000\n')

def renderer(fid,go,spritepath=block,spritefid=21300000,color=(1,1,1),order=0):
    return section(212,fid,'SpriteRenderer',f'  m_GameObject: {{fileID: {go}}}\n  m_Enabled: 1\n  m_CastShadows: 0\n  m_ReceiveShadows: 0\n  m_DynamicOccludee: 1\n  m_Materials:\n  - {ref(material,2100000)}\n  m_SortingLayerID: 0\n  m_SortingLayer: 0\n  m_SortingOrder: {order}\n  m_Sprite: {ref(spritepath,spritefid)}\n  m_Color: {{r: {color[0]}, g: {color[1]}, b: {color[2]}, a: 1}}\n  m_FlipX: 0\n  m_FlipY: 0\n  m_DrawMode: 0\n  m_Size: {{x: 1, y: 1}}\n  m_WasSpriteAssigned: 1\n  m_MaskInteraction: 0\n  m_SpriteSortPoint: 0\n')

# Allocate four unused native layers, preserving all existing project layers.
tagpath=ROOT/'ProjectSettings/TagManager.asset'
tagtext=tagpath.read_text()
start=tagtext.index('  layers:\n')+len('  layers:\n')
end=tagtext.index('  m_SortingLayers:',start)
layers=tagtext[start:end].splitlines()
assigned=[]
for name in ['EchoSolid','EchoPlatform','EchoHazard','EchoPlayer']:
    index=next((i for i,s in enumerate(layers) if s.strip()=='- '+name),None)
    if index is None:
        index=next(i for i in range(8,32) if layers[i].strip()=='-')
        layers[index]='  - '+name
    assigned.append(index)
write(tagpath,tagtext[:start]+'\n'.join(layers)+'\n'+tagtext[end:])
solid,platform,hazard,playerlayer=assigned

tuning=BASE/'Data/MovementTuning.asset'
tuningtext=script(11400000,0,'MovementTuning','  size: {x: 0.22, y: 0.4}\n')
tuningtext=tuningtext.replace('  m_Name: \n','  m_Name: MovementTuning\n')
for key,value in re.findall(r'public float (\w+) = ([\d.]+)f;', (BASE/'Scripts/MovementTuning.cs').read_text()): tuningtext+=f'  {key}: {float(value)}\n'
write(tuning,HEADER+tuningtext)
meta(tuning,'NativeFormatImporter','  mainObjectFileID: 11400000\n')

def uuid(label): return hashlib.md5(label.encode()).hexdigest()
actions=[]; bindings=[]
for name,typ,expected in [('Move','Value','Vector2'),('Jump','Button','Button'),('Dash','Button','Button'),('Reset','Button','Button')]:
    actions.append(dict(name=name,type=typ,id=uuid(name),expectedControlType=expected,processors='',interactions='',initialStateCheck=typ=='Value'))
def binding(action,path,name='',composite=False,part=False):
    bindings.append(dict(name=name,id=uuid(str(len(bindings))+path),path=path,interactions='',processors='',groups='',action=action,isComposite=composite,isPartOfComposite=part))
for keys in [('w','s','a','d'),('upArrow','downArrow','leftArrow','rightArrow')]:
    binding('Move','2DVector',composite=True)
    for name,key in zip(('Up','Down','Left','Right'),keys): binding('Move','<Keyboard>/'+key,name,part=True)
for path in ['<Gamepad>/leftStick','<Gamepad>/dpad']: binding('Move',path)
for action,paths in {'Jump':['<Keyboard>/space','<Gamepad>/buttonSouth'],'Dash':['<Keyboard>/k','<Keyboard>/leftShift','<Keyboard>/rightShift','<Gamepad>/buttonEast'],'Reset':['<Keyboard>/r','<Gamepad>/select']}.items():
    for path in paths: binding(action,path)
inputpath=BASE/'Input/Movement.inputactions'
write(inputpath,json.dumps(dict(name='Movement',maps=[dict(name='Movement',id=uuid('Movement map'),actions=actions,bindings=bindings)],controlSchemes=[]),indent=2)+'\n')
meta(inputpath,'ScriptedImporter','  internalIDToNameTable: []\n  serializedVersion: 2\n  script: {fileID: 11500000, guid: 8404be70184654265930450def6a9037, type: 3}\n  generateWrapperCode: 0\n')

prefab=HEADER+gameobject(1,'Wanderer',[2,3,4,5,6],playerlayer)+transform(2,1,children=[12])
prefab+=section(50,3,'Rigidbody2D','  serializedVersion: 5\n  m_GameObject: {fileID: 1}\n  m_BodyType: 1\n  m_Simulated: 1\n  m_UseFullKinematicContacts: 0\n  m_UseAutoMass: 0\n  m_Mass: 1\n  m_LinearDamping: 0\n  m_AngularDamping: 0.05\n  m_GravityScale: 0\n  m_Material: {fileID: 0}\n  m_Interpolate: 0\n  m_SleepingMode: 1\n  m_CollisionDetection: 1\n  m_Constraints: 4\n')
prefab+=collider(4,1,(.22,.4),(0,.2))
prefab+=script(5,1,'PlayerMotor',f'  tuning: {ref(tuning)}\n  solids:\n    serializedVersion: 2\n    m_Bits: {1<<solid}\n  oneWayPlatforms:\n    serializedVersion: 2\n    m_Bits: {1<<platform}\n  hazards:\n    serializedVersion: 2\n    m_Bits: {1<<hazard}\n  roomBounds: {{x: 0, y: -0.88, width: 32, height: 12}}\n  automaticSimulation: 1\n')
prefab+=script(6,1,'MovementInput',f'  actions: {ref(inputpath,-944628639613478452)}\n')
prefab+=gameobject(11,'Visual',[12,13,14],playerlayer)+transform(12,11,scale=(.34,.34,.34),parent=2)
prefab+=renderer(13,11,atlas,21300000,order=10)
prefab+=script(14,11,'PlayerVisual','  motor: {fileID: 5}\n  frames:\n'+''.join(f'  - {ref(atlas,21300000+i)}\n' for i in range(40)))
write(PREFAB,prefab); meta(PREFAB,'PrefabImporter')

sample=(ROOT/'Assets/Scenes/SampleScene.unity').read_text()
scene=sample[:sample.index('--- !u!1 &')]
scene+=f'--- !u!1001 &10000\nPrefabInstance:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n  m_Modification:\n    serializedVersion: 3\n    m_TransformParent: {{fileID: 0}}\n    m_Modifications:\n    - target: {ref(PREFAB,2)}\n      propertyPath: m_LocalPosition.x\n      value: 1.21\n      objectReference: {{fileID: 0}}\n    - target: {ref(PREFAB,2)}\n      propertyPath: m_LocalPosition.y\n      value: 0.02\n      objectReference: {{fileID: 0}}\n    m_RemovedComponents: []\n    m_RemovedGameObjects: []\n    m_AddedGameObjects: []\n    m_AddedComponents: []\n  m_SourcePrefab: {ref(PREFAB,100100000)}\n'
scene+=f'--- !u!4 &10002 stripped\nTransform:\n  m_CorrespondingSourceObject: {ref(PREFAB,2)}\n  m_PrefabInstance: {{fileID: 10000}}\n  m_PrefabAsset: {{fileID: 0}}\n'
scene+=f'--- !u!114 &10005 stripped\nMonoBehaviour:\n  m_CorrespondingSourceObject: {ref(PREFAB,5)}\n  m_PrefabInstance: {{fileID: 10000}}\n  m_PrefabAsset: {{fileID: 0}}\n  m_GameObject: {{fileID: 0}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {ref(BASE/"Scripts/PlayerMotor.cs",11500000)}\n  m_Name: \n  m_EditorClassIdentifier: \n'
scene+=gameobject(2000,'Main Camera',[2001,2002,2003,2004],tag='MainCamera')+transform(2001,2000,pos=(4.8,1.82,-10))
cam=re.search(r'--- !u!20 &519420031\nCamera:.*?(?=--- !u!)',sample,re.S).group(0)
cam=cam.replace('&519420031','&2002').replace('fileID: 519420028','fileID: 2000').replace('orthographic size: 5','orthographic size: 2.7')
cam=re.sub(r'm_BackGroundColor: .*', 'm_BackGroundColor: {r: 0.035, g: 0.06, b: 0.1, a: 1}',cam)
scene+=cam+section(81,2003,'AudioListener','  m_GameObject: {fileID: 2000}\n  m_Enabled: 1\n')
scene+=script(2004,2000,'RoomCamera','  target: {fileID: 10005}\n  bounds: {x: 0, y: -0.88, width: 32, height: 12}\n  smoothing: 7\n  lookAheadSeconds: 0.13\n')
roots=[10002,2001]; nextid=3000
def box(name,rect,layer,color):
    global nextid,scene
    go=nextid; nextid+=10; roots.append(go+1)
    x,y,w,h=rect
    scene+=gameobject(go,name,[go+1,go+2,go+3],layer)+transform(go+1,go,pos=(x+w/2,y+h/2,0))
    # Unit sprite scaled on the transform; matching local collider dimensions.
    scene=scene.replace(f'  m_LocalPosition: {{x: {x+w/2}, y: {y+h/2}, z: 0}}\n  m_LocalScale: {{x: 1, y: 1, z: 1}}',f'  m_LocalPosition: {{x: {x+w/2}, y: {y+h/2}, z: 0}}\n  m_LocalScale: {{x: {w}, y: {h}, z: 1}}')
    scene+=renderer(go+2,go,color=color)+collider(go+3,go,(1,1),trigger=layer!=solid)
solidcolor=(.23,.32,.40); platformcolor=(.32,.66,.66)
for name,rect in [('West floor',(0,-1,14,1)),('East floor',(15.5,-1,16.5,1)),('West boundary',(-.3,-1,.3,13)),('East boundary',(32,-1,.3,13)),('Dash stop wall',(8.9,0,.28,1.4)),('Low solid ceiling',(10.2,1.05,2,.3))]: box(name,rect,solid,solidcolor)
for name,rect in [('Platform 1',(2.65,.7,1.4,.2)),('Platform 2',(4.55,1.6,1.45,.2)),('Platform 3',(6.6,2.5,1.7,.2)),('Descent 1',(25.7,6.4,1.6,.2)),('Descent 2',(28.2,4.4,1.6,.2)),('Descent 3',(30,2.4,1.6,.2))]: box(name,rect,platform,platformcolor)
box('Hazard reset pit',(14,-.7,1.5,.25),hazard,(.94,.3,.43))
for name,x,y,w,h,layer in [('Shaft approach',210,355,140,20,platform),('Shaft entry',335,255,165,20,platform),('Belfry left wall',345,-360,28,590,solid),('Belfry right wall',520,-490,28,740,solid),('Shaft mid rest',335,-195,155,20,platform),('Summit landing',570,-360,230,22,platform)]:
    box(name,(17+x/100,(452-y-h)/100,w/100,h/100),layer,platformcolor if layer==platform else solidcolor)
for words,pos in [('01 / VARIABLE JUMP',(.65,1.55)),('02 / ONE-WAY PLATFORMS',(3,3.25)),('03 / WALL + CEILING',(8.8,2.05)),('04 / GAP + SAFE RESET',(12.85,1)),('05 / BELFRY ASCENT',(18.65,3.3)),('WALL JUMP\nHold toward wall; tap Jump',(18.4,5.2)),('SUMMIT / AIR DASH ACROSS',(22.7,8.75)),('06 / DROP + LAND',(27.5,5.2))]:
    go=nextid;nextid+=10;roots.append(go+1)
    scene+=gameobject(go,words.split('\n')[0],[go+1,go+2,go+3,go+4])+transform(go+1,go,pos=(*pos,0))
    scene+=section(102,go+2,'TextMesh',f'  serializedVersion: 3\n  m_GameObject: {{fileID: {go}}}\n  m_Text: {json.dumps(words)}\n  m_OffsetZ: 0\n  m_CharacterSize: 0.025\n  m_LineSpacing: 1\n  m_Anchor: 0\n  m_Alignment: 0\n  m_TabSize: 4\n  m_FontSize: 48\n  m_FontStyle: 0\n  m_RichText: 1\n  m_Font: {{fileID: 0}}\n  m_Color: {{r: 0.58, g: 0.79, b: 0.83, a: 1}}\n')
    scene+=section(23,go+3,'MeshRenderer',f'  m_GameObject: {{fileID: {go}}}\n  m_Enabled: 1\n  m_CastShadows: 0\n  m_ReceiveShadows: 0\n  m_Materials: []\n  m_SortingOrder: 2\n')
    scene+=script(go+4,go,'LabMarker')
scene+=gameobject(9000,'Movement HUD',[9001,9002])+transform(9001,9000)+script(9002,9000,'MovementLabHUD','  player: {fileID: 10005}\n  status: {fileID: 0}\n')
roots.append(9001)
scene+='--- !u!1660057539 &9223372036854775807\nSceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n'+''.join(f'  - {{fileID: {i}}}\n' for i in roots)
write(SCENE,scene);meta(SCENE)
buildsettings=ROOT/'ProjectSettings/EditorBuildSettings.asset'
text=buildsettings.read_text()
text=text.replace('  m_Scenes:\n',f'  m_Scenes:\n  - enabled: 1\n    path: Assets/EchoFall/Scenes/MovementLab.unity\n    guid: {guid(SCENE)}\n')
write(buildsettings,text)
timepath=ROOT/'ProjectSettings/TimeManager.asset'
text=re.sub(r'Fixed Timestep: .*','Fixed Timestep: 0.008333334',timepath.read_text())
write(timepath,re.sub(r'Maximum Allowed Timestep: .*','Maximum Allowed Timestep: 0.1',text))

for path in sorted(BASE.rglob('*')):
    if path.name.endswith('.meta') or Path(str(path)+'.meta').exists(): continue
    if path.is_dir():
        write(Path(str(path)+'.meta'),f'fileFormatVersion: 2\nguid: {guid(path)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    else: meta(path,'TextScriptImporter' if path.suffix=='.json' else 'DefaultImporter')
write(Path(str(BASE)+'.meta'),f'fileFormatVersion: 2\nguid: {guid(BASE)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n')
print('Authored MovementLab scene, Player prefab, tuning, actions, 40 sprite slices and native colliders.')
