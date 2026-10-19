"""Render owned editable Blender art; run only against the workspace copy.
Never saves or writes the original JavaScript project.
"""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/EchoFall/Resources/Presentation'
OUT.mkdir(parents=True,exist_ok=True)
s=bpy.data.scenes['01 WANDERER / animated source'];bpy.context.window.scene=s
s.cycles.samples=8
rig=[o for o in s.objects if o.name.startswith('RIG /')]
poses={}
for f in range(1,17):
    s.frame_set(f);poses[f]={o.name:(o.location.copy(),o.rotation_euler.copy()) for o in rig}
for o in rig:o.animation_data_clear()
s.frame_set(1)
def setpose(base,mods):
    for o in rig:
        p,r=poses[base][o.name];o.location=p;o.rotation_euler=r
        key=o.name.replace('RIG / ','')
        if key in mods:o.rotation_euler.y=mods[key]
    if 'z' in mods:bpy.data.objects['RIG / body root'].location.z=mods['z']
states={'Rise':4,'Fall':4,'Land':4,'WallSlide':4,'WallJump':4,'Dash':4,'Hurt':4,'Death':6,'Transfer':6,'Up':6,'Down':6,'Charged':8,'Combo2':6,'Combo3':6}
for state,count in states.items():
    dest=OUT/'Player'/state;dest.mkdir(parents=True,exist_ok=True)
    for i in range(count):
        t=i/(count-1);wave=math.sin(t*math.pi);base=13 if state in ['Rise','Fall','WallJump','Dash'] else 1
        mods={}
        if state=='Rise':mods={'near hip':-.6+t*.35,'far hip':.4-t*.2,'near knee':.85,'near shoulder':-.4,'far shoulder':.45}
        if state=='Fall':mods={'near hip':-.15,'far hip':.25,'near knee':.4,'near shoulder':-.8,'far shoulder':.75,'body root':-.08}
        if state=='Land':mods={'z':-.13*(1-t),'near hip':-.65*(1-t),'far hip':-.55*(1-t),'near knee':1.1*(1-t),'far knee':1.0*(1-t),'body root':.14*(1-t)}
        if state=='WallSlide':mods={'near shoulder':-1.6,'near elbow':-.5,'far shoulder':-1.2,'near hip':-.5,'near knee':.9,'body root':-.12}
        if state=='WallJump':mods={'near shoulder':-.5,'far shoulder':.9,'near hip':-.7*(1-t),'far hip':.5,'near knee':.8,'body root':.15*(1-t)}
        if state=='Dash':mods={'body root':.4,'near shoulder':.65,'far shoulder':.8,'near hip':-.65,'far hip':.8,'near knee':.75}
        if state=='Hurt':mods={'body root':-.23*(1-t),'head':-.15,'near shoulder':.4,'far shoulder':.5,'near knee':.25}
        if state=='Death':mods={'z':-.28*t,'body root':.65*t,'head':.35*t,'near hip':-1.2*t,'far hip':-1.0*t,'near knee':1.5*t,'far knee':1.3*t,'near shoulder':.5*t}
        if state=='Transfer':mods={'near shoulder':-.5-t*.4,'far shoulder':.4+t*.4,'head':-.15*t,'body root':-.08*t}
        if state in ['Up','Down','Charged','Combo2','Combo3']:
            base=14+min(2,int(t*2.99));mods={'body root':.1*wave}
            if state=='Up':mods.update({'near shoulder':-2.4+t*1.1,'near elbow':-.3})
            elif state=='Down':mods.update({'near shoulder':-.25+t*.6,'near elbow':-.15,'body root':.3*wave,'near hip':-.4,'near knee':.6})
            elif state=='Charged':mods.update({'near shoulder':-2.0+t*2.7,'near elbow':-.6*(1-t),'body root':.2*wave})
            elif state=='Combo2':mods.update({'near shoulder':.6-t*2.5,'near elbow':-.4,'body root':-.12*wave})
            else:mods.update({'near shoulder':-2.2+t*2.8,'near elbow':-.2,'body root':.23*wave})
        if state in ['Fall','WallSlide','Dash']:
            mods['head']=mods.get('head',0)+math.sin(t*math.tau)*.045
            mods['far knee']=mods.get('far knee',.3)+math.sin(t*math.tau)*.09
        setpose(base,mods);bpy.context.view_layer.update();s.render.filepath=str(dest/f'{i:02}.png');bpy.ops.render.render(write_still=True)
setpose(1,{});bpy.context.view_layer.update();p=world_to_camera_view(s,s.camera,Vector((0,0,0)))
(OUT/'player-manifest.json').write_text(json.dumps({'cell':s.render.resolution_x,'pivot':[p.x,p.y],'states':states},indent=2))

# Dedicated small figures and architecture, using beveled low-poly forms like the source kit.
scene=bpy.data.scenes.new('PRESENTATION / owned NPC and regional assets');bpy.context.window.scene=scene
scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=scene.render.resolution_y=512;scene.render.resolution_percentage=100;scene.render.film_transparent=True
scene.world=bpy.data.worlds.new('Presentation world');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.25,.3,.36,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
def mat(name,rgb,metal=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True;b=m.node_tree.nodes.get('Principled BSDF');b.inputs['Base Color'].default_value=(*rgb,1);b.inputs['Metallic'].default_value=metal;b.inputs['Roughness'].default_value=.65;return m
stone=mat('blue ceramic',(.19,.31,.35));pale=mat('pale ceramic',(.55,.65,.62));cloth=mat('plum cloth',(.22,.12,.19));gold=mat('aged brass',(.4,.28,.12),.65);dark=mat('deep inset',(.035,.07,.08));green=mat('leaf jade',(.16,.3,.18));bark=mat('root bark',(.12,.17,.13));glow=mat('signal glass',(.25,.75,.68))
def cube(name,loc,scale,m):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m);b=o.modifiers.new('worn bevel','BEVEL');b.width=.035;b.segments=2;o.modifiers.new('weighted normals','WEIGHTED_NORMAL');return o
def cone(name,loc,r1,r2,depth,m):
    bpy.ops.mesh.primitive_cone_add(vertices=10,radius1=r1,radius2=r2,depth=depth,location=loc);o=bpy.context.object;o.name=name;o.data.materials.append(m);return o
def rod(name,a,b,r,m):
    d=Vector(b)-Vector(a);o=cone(name,(Vector(a)+Vector(b))/2,r,r,d.length,m);o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o
def orb(name,loc,scale,m):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(m);return o
def camera():
    data=bpy.data.cameras.new('Asset camera');o=bpy.data.objects.new('Asset camera',data);scene.collection.objects.link(o);scene.camera=o;data.type='ORTHO'
    o.location=(3,-8,3);o.rotation_euler=(Vector((0,0,1.4))-o.location).to_track_quat('-Z','Y').to_euler();data.ortho_scale=3.6
    for loc,power,size in [((-3,-4,7),500,5),((4,1,5),650,4)]:
        data=bpy.data.lights.new('softbox','AREA');data.energy=power;data.shape='DISK';data.size=size;l=bpy.data.objects.new('softbox',data);scene.collection.objects.link(l);l.location=loc;l.rotation_euler=(Vector((0,0,1))-l.location).to_track_quat('-Z','Y').to_euler()
camera();assets={}
for kind in ['keeper','child','creature','tree','vessel','cage','cage-open','plinth']:
    for o in list(scene.objects):
        if o.type=='MESH':bpy.data.objects.remove(o,do_unlink=True)
    if kind in ['keeper','child']:
        small=kind=='child';h=1.0 if small else 1.5
        cone('pleated coat',(0,0,h*.42),.32 if small else .38,.16,h*.8,cloth if not small else green)
        orb('hood',(0,0,h),(.26,.23,.29),stone if not small else pale);cube('shadowed face',(0,-.21,h),(.23,.05,.14),dark)
        cube('quiet visor',(0,-.245,h+.015),(.15,.025,.025),glow)
        for x in [-.12,.12]:cube('boots',(x,-.05,.05),(.14,.25,.1),dark)
        for side in [-1,1]:rod('sleeve',(side*.16,0,h*.75),(side*.31,-.08,h*.4),.085,pale if small else stone)
        if not small:
            rod('signal staff',(.44,0,.05),(.44,0,1.85),.035,gold);cube('lantern frame',(.44,0,1.5),(.22,.19,.31),gold);cube('lantern glass',(.44,-.105,1.5),(.12,.025,.2),glow)
            for z in [.3,.55,.8]:cube('coat clasp',(0,-.21,z),(.075,.03,.04),gold)
    elif kind=='creature':
        orb('armored shell',(0,0,.4),(.55,.28,.28),stone)
        for x in [-.3,0,.3]:orb('shell plate',(x,0,.5),(.21,.31,.2),pale)
        orb('head',(.5,-.02,.28),(.18,.18,.17),gold);orb('eye',(.56,-.16,.32),(.035,.03,.035),glow)
        for x in [-.35,-.05,.25]:
            for y in [-1,1]:rod('leg',(x,y*.18,.3),(x+.16,y*.4,.04),.035,dark)
        rod('feelers',(.57,0,.35),(.8,-.1,.6),.018,gold)
    elif kind=='tree':
        rod('ancient trunk',(0,0,0),(.12,0,2.5),.18,bark)
        for i in range(7):
            z=.7+i*.28;side=-1 if i%2 else 1;end=(side*(.65+(i%3)*.12),0,z+.35)
            rod('branch',(.07,0,z),end,.065,bark)
            for n in range(3):orb('leaf crown',(end[0]+(n-1)*.18,0,end[2]+n*.1),(.35,.13,.15),green)
        for side in [-1,1]:rod('exposed root',(0,0,.25),(side*.65,0,.02),.09,bark)
    elif kind in ['cage','cage-open','vessel']:
        vessel=kind=='vessel';r=.45
        cone('lower ring',(0,0,.3),r,r,.10,gold);cone('upper ring',(0,0,1.5),r,r,.1,gold)
        cone('crown',(0,0,1.67),r,.1,.24,stone)
        for i in range(12):
            a=i*math.tau/12
            if kind=='cage-open' and math.sin(a)<-.4:continue
            rod('rib',(math.cos(a)*r,math.sin(a)*r,.3),(math.cos(a)*r,math.sin(a)*r,1.5),.025,stone)
        if vessel:orb('ceramic lung',(0,0,.95),(.35,.3,.58),cloth);rod('outlet',(0,0,.3),(0,0,0),.1,gold)
        elif kind=='cage':orb('borrowed light',(0,0,.6),(.1,.1,.18),glow)
        rod('suspension',(0,0,1.8),(0,0,2.5),.018,gold)
    else:
        for z,w in [(.1,1.25),(.3,1.1),(.5,1),(.7,.9),(.9,.8),(1.1,.8),(1.3,1.15)]:cube('observatory masonry',(0,0,z),(w,.65,.19),stone)
        for x in [-.38,.38]:rod('brass strut',(x,-.4,.2),(x,-.4,1.25),.025,gold)
        orb('lens focus',(0,0,1.6),(.16,.16,.3),glow)
    bpy.context.view_layer.update();p=world_to_camera_view(scene,scene.camera,Vector((0,0,0)))
    path=OUT/'Props';path.mkdir(exist_ok=True);scene.render.filepath=str(path/(kind+'.png'));bpy.ops.render.render(write_still=True);assets[kind]={'pivot':[p.x,p.y]}
(OUT/'props-manifest.json').write_text(json.dumps(assets,indent=2))
print('PRESENTATION_ART_COMPLETE',flush=True)
