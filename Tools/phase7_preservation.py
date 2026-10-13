"""Read-only Phase 6 scene/movement and original-source preservation audit."""
import pathlib, subprocess, hashlib, json
root=pathlib.Path(__file__).resolve().parents[1]
original=pathlib.Path(r'C:\Users\sivas\echo-fall')
checks=[]
for manifest in ['phase2-source-hashes.json','phase3-source-hashes.json','phase4-source-hashes.json']:
    for name,expected in json.loads((root/'Docs/Validation'/manifest).read_text(encoding='utf-8-sig')).items():
        checks.append(dict(source=name,unchanged=hashlib.sha256((original/name).read_bytes()).hexdigest()==expected))
for path in list((root/'Assets/EchoFall/Scenes').glob('*.unity'))+[root/'Assets/EchoFall/Scripts'/n for n in ['MovementTuning.cs','PlayerMotor.cs','CombatRules.cs','EndingRules.cs','SliceMemory.cs','SliceCheckpoint.cs']]:
    rel=path.relative_to(root).as_posix()
    before=subprocess.check_output(['git','show','HEAD:'+rel],cwd=root).decode('utf-8-sig').replace('\r\n','\n')
    checks.append(dict(baseline=rel,unchanged=before==path.read_text(encoding='utf-8-sig')))
(root/'Docs/Validation/phase7/preservation.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
failed=[x for x in checks if not x['unchanged']]
print(json.dumps(dict(comparisons=len(checks),failures=failed),indent=2))
if failed: raise SystemExit(1)
