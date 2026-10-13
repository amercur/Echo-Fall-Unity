"""Read-only reference, baseline and Mother collision audit."""
import pathlib,subprocess,hashlib,json,re
root=pathlib.Path(__file__).resolve().parents[1];original=pathlib.Path(r'C:\Users\sivas\echo-fall');checks=[]
for manifest in ['phase2-source-hashes.json','phase3-source-hashes.json','phase4-source-hashes.json']:
 for name,expected in json.loads((root/'Docs/Validation'/manifest).read_text(encoding='utf-8-sig')).items():
  checks.append(dict(source=name,unchanged=hashlib.sha256((original/name).read_bytes()).hexdigest()==expected))
for path in list((root/'Assets/EchoFall/Scenes').glob('*.unity'))+[root/'Assets/EchoFall/Scripts'/n for n in ['MovementInput.cs','MovementTuning.cs','PlayerMotor.cs','PlayerVisual.cs','SlicePlayerPresentation.cs']]:
 rel=path.relative_to(root).as_posix();before=subprocess.check_output(['git','show','HEAD:'+rel],cwd=root).decode('utf-8-sig').replace('\r\n','\n');after=path.read_text(encoding='utf-8-sig')
 if path.name=='Wake_mother.unity':
  blocks=lambda s:re.findall(r'--- !u!61 &[^\n]+\n.*?(?=--- !u!|\Z)',s,re.S)
  checks.append(dict(scene=rel,collisionUnchanged=blocks(before)==blocks(after)))
 else:checks.append(dict(baseline=rel,unchanged=before==after))
checks.append(dict(asset='mother.png',unchanged=(original/'assets/mother.png').read_bytes()==(root/'Assets/EchoFall/Art/Wake/mother.png').read_bytes()))
(root/'Docs/Validation/phase6/preservation.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
failed=[x for x in checks if not x.get('unchanged',x.get('collisionUnchanged',True))];print(json.dumps(dict(comparisons=len(checks),failures=failed),indent=2))
if failed:raise SystemExit(1)
