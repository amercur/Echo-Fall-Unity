"""Read-only comparison against the committed Phase 3 baseline."""
import pathlib, subprocess, hashlib, json, re
root=pathlib.Path(__file__).resolve().parents[1]
original=pathlib.Path(r'C:\Users\sivas\echo-fall')
checks=[]
for manifest in ['phase2-source-hashes.json','phase3-source-hashes.json','phase4-source-hashes.json']:
    for name,expected in json.loads((root/'Docs/Validation'/manifest).read_text(encoding='utf-8-sig')).items():
        checks.append(dict(source=name,unchanged=hashlib.sha256((original/name).read_bytes()).hexdigest()==expected))
for name in ['PlayerMotor.cs','MovementTuning.cs','MovementInput.cs','PlayerVisual.cs','SlicePlayerPresentation.cs','SliceCombat.cs','SliceEnemy.cs','SliceKing.cs','SliceProjectile.cs','SliceMemory.cs']:
    path='Assets/EchoFall/Scripts/'+name
    baseline=subprocess.check_output(['git','show','HEAD:'+path],cwd=root).replace(b'\r\n',b'\n')
    checks.append(dict(baseline=path,unchanged=baseline==(root/path).read_bytes().replace(b'\r\n',b'\n')))
for name in ['MovementLab','WakeSlice','Wake_wake','Wake_belfry','Wake_cistern','Wake_archive','Wake_procession','Wake_king']:
    path='Assets/EchoFall/Scenes/'+name+'.unity'
    before=subprocess.check_output(['git','show','HEAD:'+path],cwd=root).decode().replace('\r\n','\n')
    after=(root/path).read_text(encoding='utf-8-sig')
    colliders=lambda t:re.findall(r'--- !u!61 &[^\n]+\n.*?(?=--- !u!|\Z)',t,re.S)
    checks.append(dict(scene=path,wholeSceneUnchanged=before==after,collisionUnchanged=colliders(before)==colliders(after)))
(root/'Docs/Validation/phase4-preservation.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
fail=[c for c in checks if c.get('unchanged',True)==False or c.get('collisionUnchanged',True)==False]
print(json.dumps(dict(comparisons=len(checks),failures=fail),indent=2))
if fail:raise SystemExit(1)
