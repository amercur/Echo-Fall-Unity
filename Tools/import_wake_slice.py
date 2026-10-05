"""Read-only reference import. Run from the Unity project; never runs source generators."""
import json, pathlib, hashlib, shutil

source = pathlib.Path(r'C:\Users\sivas\echo-fall')
root = pathlib.Path(__file__).resolve().parents[1]
dest = root / 'Assets/EchoFall/Art/Wake'
dest.mkdir(parents=True, exist_ok=True)
used = ['game.js', 'data/world.json', 'assets/art-manifest.js']
for name in ['ruin-arch.png', 'ruin-ledge.png', 'choir-spire.png', 'sentinel.png', 'lancer.png', 'drone.png']:
    shutil.copyfile(source / 'assets' / name, dest / name)
    used.append('assets/' + name)
world = json.loads((source / 'data/world.json').read_text(encoding='utf-8'))
art = json.loads((source / 'assets/art-manifest.js').read_text(encoding='utf-8').split('window.ECHO_ART = ', 1)[1].strip().rstrip(';'))
maps = {m['id']: m for m in art['maps']}
rooms = []
for r in world['rooms']:
    if r['id'] not in ['wake', 'belfry', 'cistern', 'archive', 'procession']: continue
    out = {k:r[k] for k in ['id','name','width','top','bottom','enemies','interactions','tutorials']}
    for key in ['ground','platforms','solids','hazards','pogo']:
        out[key] = [dict(zip(['x','y','w','h'], rect)) for rect in maps[r['id']].get(key, r.get(key, []))]
    out['spawns'] = [dict(id=k, x=v[0]+11, y=v[1]+40) for k,v in r['spawns'].items()]
    out['decor'] = [dict(kind=d[0],x=d[1],y=d[2],scale=d[3]) for d in r['decor']]
    rooms.append(out)
(root / 'Assets/EchoFall/Data/WakeSource.json').write_text(json.dumps({'rooms':rooms},indent=2),encoding='utf-8')
proof = {name:hashlib.sha256((source / name).read_bytes()).hexdigest() for name in used}
(root / 'Docs/Validation/phase2-source-hashes.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
print('Imported 5 effective room layouts and 6 original art assets; original files unchanged.')
