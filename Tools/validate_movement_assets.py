"""Read-only structural validation; does not substitute for Unity import/play tests."""
from pathlib import Path
import hashlib, json, re, struct, subprocess

root=Path(__file__).resolve().parents[1]
base=root/'Assets/EchoFall'
guids={}
assemblies=set()
asset_files=[base/'Scenes/MovementLab.unity',base/'Prefabs/Player.prefab',base/'Data/MovementTuning.asset',base/'Materials/MovementUnlit.mat']
needed_guids=set(re.findall(r'guid: ([a-f0-9]{32})','\n'.join(p.read_text() for p in asset_files)))
# rg avoids reading tens of thousands of unrelated package metadata files in Python.
definition_paths=subprocess.check_output(['rg','--files',str(root/'Assets'),str(root/'Library/PackageCache'),'-g','*.asmdef'],text=True).splitlines()
for filename in definition_paths:
    assemblies.add(json.loads(Path(filename).read_text(encoding='utf-8-sig'))['name'])
metadata_paths=subprocess.check_output(['rg','-l','-g','*.meta','guid: ('+'|'.join(sorted(needed_guids))+')',str(root/'Assets'),str(root/'Library/PackageCache')],text=True).splitlines()
for filename in metadata_paths:
    meta=Path(filename)
    match=re.search(r'^guid: ([a-f0-9]{32})$',meta.read_text(encoding='utf-8-sig',errors='replace'),re.M)
    if match: guids[match[1]]=meta
for path in base.rglob('*'):
    if path.name.endswith('.meta'): continue
    assert Path(str(path)+'.meta').exists(), f'Missing meta: {path}'
for path in asset_files:
    text=path.read_text()
    ids=re.findall(r'^--- !u!\d+ &(\d+)',text,re.M)
    assert len(ids)==len(set(ids)),f'Duplicate fileID: {path}'
    for fid in re.findall(r'\{fileID: (\d+)\}',text):
        assert fid=='0' or fid in ids, f'Missing internal fileID {fid}: {path}'
    for guid in re.findall(r'guid: ([a-f0-9]{32})',text):
        assert guid in {'0'*32, '0000000000000000e000000000000000', '0000000000000000f000000000000000'} or guid in guids, f'Missing GUID {guid}: {path}'
    print(f'PASS {path.relative_to(root)}: {len(ids)} objects, references resolve')
for definition in base.rglob('*.asmdef'):
    for reference in json.loads(definition.read_text())['references']:
        assert reference in assemblies, f'Unknown assembly reference {reference}: {definition}'
print('PASS Unity assembly definition references')
atlas=base/'Art/Wanderer/wanderer-smooth.png'
width,height=struct.unpack('>II',atlas.read_bytes()[16:24])
assert (width,height)==(768,1920)
metadata=Path(str(atlas)+'.meta').read_text()
assert len(re.findall(r'^      name: Wanderer_\d+',metadata,re.M))==40
assert len(re.findall(r'^      internalID: 213000\d\d',metadata,re.M))==40
prefab=(base/'Prefabs/Player.prefab').read_text()
for i in range(40): assert f'fileID: {21300000+i}, guid: ' in prefab
actions=json.loads((base/'Input/Movement.inputactions').read_text())
assert {a['name'] for a in actions['maps'][0]['actions']}=={'Move','Jump','Dash','Reset'}
assert '<Keyboard>/space' in {b['path'] for b in actions['maps'][0]['bindings']}
assert '<Gamepad>/buttonSouth' in {b['path'] for b in actions['maps'][0]['bindings']}
assert '6000.5.8f1' in (root/'ProjectSettings/ProjectVersion.txt').read_text()
assert 'Fixed Timestep: 0.008333334' in (root/'ProjectSettings/TimeManager.asset').read_text()
assert 'Assets/EchoFall/Scenes/MovementLab.unity' in (root/'ProjectSettings/EditorBuildSettings.asset').read_text()
for filename,expected in {
    'wanderer-smooth.png':'4033D3A19057F7E80FDBAE0212FE5594F6AD1DE31806DDC0170F9A8E1262C548',
    'wanderer-smooth.json':'40BCAB87760F0829A29442BF4BD89FE74D9461557417B94746FF2B987F69EA9D'
}.items():
    assert hashlib.sha256((base/'Art/Wanderer'/filename).read_bytes()).hexdigest().upper()==expected
print('PASS atlas frames/pivots references, input bindings, Unity version, fixed timestep, build scene and source asset hashes')
print('Unity import and play verification are separate, still required checks.')
