"""Read-only Phase 4 import. Effective source geometry plus explicit exploration adaptations."""
import json, pathlib, hashlib, copy
root=pathlib.Path(__file__).resolve().parents[1]
source=pathlib.Path(r'C:\Users\sivas\echo-fall')
world=json.loads((source/'data/world.json').read_text(encoding='utf-8'))
art=json.loads((source/'assets/art-manifest.js').read_text(encoding='utf-8').split('window.ECHO_ART = ',1)[1].strip().rstrip(';'))
rooms=copy.deepcopy(world['rooms'])
newids=['cradle','lungs','mother','garden','observatory','choir']
def item(room,id): return next(i for i in room['interactions'] if i['id']==id)
def gate(id,x,y,target,entry,label,**kw): return dict(id=id,kind='gate',x=x,y=y,target=target,entry=entry,label=label,**kw)
for r in rooms:
    if r['id']=='mother':
        r['name']='THE IMMORTAL CHAMBER / THE SLEEPING ENGINE'
        item(r,'mother-east')['requires']='garden-path'
        r['interactions'] += [dict(id='garden-path-lever',kind='lever',x=285,y=170,flag='garden-path',label='RELEASE THE GARDEN CAUSEWAY'),dict(id='sleeping-engine',kind='lore',x=650,y=417,label='A HEART WITHOUT A BEAT',story='The sovereign sleeps below. A maintenance release above the western stair opens the garden causeway.')]
        r['enemies']=[dict(type='sentinel',x=970,floor=452,left=900,right=1060),dict(type='drone',x=1030,floor=340,left=940,right=1140)]
    if r['id']=='lungs':
        i=item(r,'lungs-archive');i.pop('memory',None);i.pop('blockedBy',None);i['requires']='breath-link'
        r['interactions'].append(dict(id='breath-lever',kind='lever',x=1060,y=190,flag='breath-link',label='UNLATCH THE BREATHING PASSAGE'))
    if r['id']=='archive':
        for i in r['interactions']:
            if i.get('target')=='lungs': i.pop('memory',None);i.pop('blockedBy',None);i['requires']='breath-link'
    if r['id']=='garden':
        r['interactions'].append(gate('garden-vault',785,125,'rootvault','entry','A HOLLOW BETWEEN THE ROOTS'))
    if r['id']=='choir':
        item(r,'choir')['kind']='lore';item(r,'choir')['story']='The voices gather around an empty place. The final choice waits beyond this exploration milestone.'
        r['interactions'].append(dict(id='choir-rest',kind='bench',x=820,y=425,label='SIGNAL ANCHOR'))
    for i in r['interactions']:
        if i['kind'] in ['keeper','echo','body','style']:
            original_kind=i['kind'];i['kind']='lore';i['story']='The Keeper tends the sleeping engine. Climb to the Lungs; their maintenance lift returns above the chamber.' if original_kind=='keeper' else 'An old record hints at paths above. Forms and further identities await a later milestone.'
vault=dict(id='rootvault',name='THE ROOT VAULT / WHAT THE GARDEN HIDES',area=2,width=800,top=-80,bottom=540,map=[860,480],ground=[[0,452,260,100],[590,452,210,100]],platforms=[[300,340,95,16],[430,245,100,16]],solids=[],hazards=[[260,510,330,30]],pogo=[[375,405,40,20]],enemies=[dict(type='drone',x=465,floor=200,left=420,right=540)],interactions=[gate('vault-return',70,417,'garden','secret','RETURN TO THE GARDEN'),dict(id='vault-cache',kind='cache',x=670,y=417,label='THE UNOPENED SEED',story='Someone hid this pulse where only a falling blade could wake the roots.'),dict(id='vault-record',kind='relic',x=475,y=210,label='THE GARDENER WHO REFUSED',story='Above the machinery, they planted something that could not be commanded.')],spawns={'default':[100,412],'entry':[100,412]},decor=[])
rooms.append(vault);newids.append('rootvault')
next(r for r in rooms if r['id']=='garden')['spawns']['secret']=[780,120]
normalized=[]
for r in rooms:
    m=next((m for m in art['maps'] if m['id']==r['id']),{})
    out={k:r[k] for k in ['id','name','width','top','bottom','enemies','interactions']}
    for key in ['ground','platforms','solids','hazards','pogo']:out[key]=[dict(zip(['x','y','w','h'],rect)) for rect in m.get(key,r.get(key,[]))]
    # The source's 137px chamber step exceeds the unchanged 124px jump apex.
    # A maintenance tread makes the exploration bypass reachable without a new ability.
    if r['id']=='mother': out['platforms'].append(dict(x=290,y=285,w=90,h=16))
    out['spawns']=[dict(id=k,x=v[0]+11,y=v[1]+40) for k,v in r['spawns'].items()]
    out['decor']=[dict(kind=d[0],x=d[1],y=d[2],scale=d[3]) for d in r.get('decor',[])]
    if r['id'] in newids: normalized.append(out)
catalog=[]
for r in rooms:
    catalog.append(dict(id=r['id'],name=r['name'].split(' / ')[0],region=['Wake','Cradle','Last Garden','Choir'][r['area']],x=r['map'][0],y=r['map'][1],hidden=r['id']=='rootvault',benches=[i['id'] for i in r['interactions'] if i['kind']=='bench'],collectibles=[i['id'] for i in r['interactions'] if i['kind'] in ['cache','relic']],enemies=[dict(id=r['id']+'/enemy-'+str(n),hp={'drone':5,'lancer':10}.get(e['type'],7)) for n,e in enumerate(r['enemies'])]+([dict(id='king/boss',hp=40)] if r['id']=='king' else []),links=[dict(id=i['id'],target=i['target'],entry=i.get('entry','default'),requires=i.get('requires',''),memory=i.get('memory',''),blockedBy=i.get('blockedBy','')) for i in r['interactions'] if i['kind']=='gate']))
(root/'Assets/EchoFall/Resources').mkdir(exist_ok=True)
(root/'Assets/EchoFall/Resources/WorldCatalog.json').write_text(json.dumps(dict(rooms=catalog,flags=['sluice','archive-lift','king','lung-lift','well-link','breath-link','garden-path','child']),indent=2),encoding='utf-8')
(root/'Assets/EchoFall/Data/WorldExpansion.json').write_text(json.dumps(dict(rooms=normalized),indent=2),encoding='utf-8')
proof={n:hashlib.sha256((source/n).read_bytes()).hexdigest() for n in ['game.js','data/world.json','assets/art-manifest.js']}
(root/'Docs/Validation/phase4-source-hashes.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
print('Imported:',', '.join(newids))
