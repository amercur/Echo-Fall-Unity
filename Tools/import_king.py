"""Read-only Phase 3 reference import; does not rebuild Phase 2 assets."""
import json,pathlib,shutil,hashlib
root=pathlib.Path(__file__).resolve().parents[1]
source=pathlib.Path(r'C:\Users\sivas\echo-fall')
world=json.loads((source/'data/world.json').read_text(encoding='utf-8'))
art=json.loads((source/'assets/art-manifest.js').read_text(encoding='utf-8').split('window.ECHO_ART = ',1)[1].strip().rstrip(';'))
r=next(r for r in world['rooms'] if r['id']=='king');m=next(m for m in art['maps'] if m['id']=='king')
out={k:r[k] for k in ['id','name','width','top','bottom','enemies','interactions']}
for key in ['ground','platforms','solids','hazards','pogo']:out[key]=[dict(zip(['x','y','w','h'],rect)) for rect in m.get(key,r.get(key,[]))]
out['spawns']=[dict(id=k,x=v[0]+11,y=v[1]+40) for k,v in r['spawns'].items()]
out['decor']=[dict(kind=d[0],x=d[1],y=d[2],scale=d[3]) for d in r['decor']]
(root/'Assets/EchoFall/Data/KingSource.json').write_text(json.dumps({'rooms':[out]},indent=2),encoding='utf-8')
shutil.copyfile(source/'assets/king.png',root/'Assets/EchoFall/Art/Wake/king.png')
proof={n:hashlib.sha256((source/n).read_bytes()).hexdigest() for n in ['game.js','data/world.json','assets/art-manifest.js','assets/king.png']}
(root/'Docs/Validation/phase3-source-hashes.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
print(json.dumps(art['king'],indent=2))
