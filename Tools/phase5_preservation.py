"""Read-only source and authored-world preservation audit against Phase 4 HEAD."""
import pathlib, subprocess, hashlib, json
root=pathlib.Path(__file__).resolve().parents[1]
original=pathlib.Path(r'C:\Users\sivas\echo-fall')
checks=[]
for manifest in ['phase2-source-hashes.json','phase3-source-hashes.json','phase4-source-hashes.json']:
 for name,expected in json.loads((root/'Docs/Validation'/manifest).read_text(encoding='utf-8-sig')).items():
  checks.append(dict(source=name,unchanged=hashlib.sha256((original/name).read_bytes()).hexdigest()==expected))
paths=list((root/'Assets/EchoFall/Scenes').glob('*.unity'))
paths += [root/'Assets/EchoFall/Scripts'/name for name in ['MovementTuning.cs','MovementInput.cs','PlayerVisual.cs','SlicePlayerPresentation.cs','SliceProjectile.cs']]
for path in paths:
 relative=path.relative_to(root).as_posix()
 before=subprocess.check_output(['git','show','HEAD:'+relative],cwd=root).replace(b'\r\n',b'\n')
 checks.append(dict(baseline=relative,unchanged=before==path.read_bytes().replace(b'\r\n',b'\n')))
output=root/'Docs/Validation/phase5/preservation.json';output.write_text(json.dumps(checks,indent=2),encoding='utf-8')
print(json.dumps(dict(comparisons=len(checks),failures=[c for c in checks if not c['unchanged']]),indent=2))
if any(not c['unchanged'] for c in checks):raise SystemExit(1)
