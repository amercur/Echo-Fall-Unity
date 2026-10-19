"""Original deterministic sound design and score; no sampled third-party material."""
from pathlib import Path
import math,random,wave,array,json
ROOT=Path(__file__).resolve().parents[1]/'Assets/EchoFall/Resources/Presentation/Audio'
ROOT.mkdir(parents=True,exist_ok=True)
RATE=22050;TAU=math.tau;manifest={}
def write(name,data):
    peak=max(abs(v) for v in data) or 1;gain=min(1,.82/peak)
    pcm=array.array('h',(int(max(-1,min(1,v*gain))*32767) for v in data))
    with wave.open(str(ROOT/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(RATE);w.writeframes(pcm.tobytes())
    manifest[name]={'seconds':len(data)/RATE,'peak':peak*gain,'rms':math.sqrt(sum((v*gain)**2 for v in data)/len(data))}
def tone(t,f):return math.sin(TAU*f*t)
def effect(name,seconds,freq,noise=.1,metal=False):
    rng=random.Random(name);data=[];low=0
    for i in range(int(seconds*RATE)):
        t=i/RATE;u=t/seconds;low=.65*low+.35*rng.uniform(-1,1);env=min(1,t/.004)*(1-u)**2
        v=.4*tone(t,freq*(1-.28*u))+.13*tone(t,freq*2.71)*math.exp(-t*18)+noise*low
        if metal:v+=.12*tone(t,freq*5.43)*math.exp(-t*8)
        data.append(env*v)
    write(name,data)
for n,d,f,no,m in [('stepStone',.09,72,.8,False),('stepMetal',.16,135,.3,True),('stepWood',.12,110,.45,False),('stepMoss',.12,45,1.0,False),('jump',.15,240,.2,False),('land',.2,55,.7,False),('dash',.19,170,.8,False),('slash',.14,190,.9,False),('charged',.32,95,.65,True),('impact',.18,70,.6,False),('heavyImpact',.35,43,.7,True),('deflect',.4,720,.1,True),('counter',.5,530,.15,True),('imprint',.32,960,.07,True),('detonate',.45,85,.8,True),('transfer',1.2,220,.08,True),('signal',.65,440,.05,True),('ui',.09,420,0,False),('focus',.045,630,0,False)]:effect(n,d,f,no,m)
# A restrained four-bar motif: suspended harmony, answering plucks, low rhythmic pulses.
for name,root,pace in [('exploration',110,2),('combat',110,.5),('boss',82.4069,.5),('memory',164.8138,2),('ending',130.8128,2)]:
    data=[];notes=[0,7,12,10,7,3,5,7];chords=[(0,7,12),(3,10,15),(5,12,17),(0,7,10)]
    for i in range(RATE*16):
        t=i/RATE;ch=chords[int(t/4)%4];fade=min(1,t/.04,(16-t)/.04)
        pad=sum(.065*tone(t,root*2**(n/12)) for n in ch)*(.8+.2*tone(t,.125))
        beat=t%pace;note=notes[int(t/pace)%8];pluck=.15*math.exp(-beat*5)*tone(t,root*2**(note/12)*2)
        pulse=.07*math.exp(-(t%.5)*18)*tone(t,root*.5) if pace<1 else 0
        data.append((pad+pluck+pulse)*fade)
    write(name,data)
for name,f in [('wakeAmbience',55),('cradleAmbience',41.25),('gardenAmbience',82.5),('choirAmbience',110)]:
    rng=random.Random(name);data=[];low=0
    for i in range(RATE*8):
        t=i/RATE;low=.98*low+.02*rng.uniform(-1,1);env=min(1,t/.05,(8-t)/.05)
        wind=low*(.6+.3*tone(t,.25));hum=.075*tone(t,f)+.025*tone(t,f*2)
        detail=.04*tone(t,660)*(max(0,tone(t,.5))**14) if name=='gardenAmbience' else .025*tone(t,f*4)*(.5+.5*tone(t,1))
        data.append((wind+hum+detail)*env)
    write(name,data)
(ROOT/'provenance.json').write_text(json.dumps({'authoring':'Original procedural composition and sound design in Tools/compose_presentation_audio.py. No third-party samples.','assets':manifest},indent=2))
print('AUDIO_COMPLETE',len(manifest))
