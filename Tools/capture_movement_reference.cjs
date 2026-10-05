'use strict';
// Reads and executes the original only in its in-memory test harness. Writes here only.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const source = process.argv[2] || 'C:/Users/sivas/echo-fall';
const {boot} = require(path.join(source, 'tests/harness.cjs'));
const definitions = [
  {name:'run-brake', frames:120, commands:i=>({move:i<72?1:0})},
  {name:'held-jump', frames:120, commands:i=>({jumpPressed:i===12})},
  {name:'short-jump', frames:120, commands:i=>({jumpPressed:i===12,jumpReleased:i===24})},
  {name:'jump-dash', frames:150, commands:i=>({move:1,jumpPressed:i===12,dashPressed:i===40 || i===115})},
  {name:'platform-drop', frames:200, platforms:[{x:0,y:362,w:600,h:20}], commands:i=>({jumpPressed:i===12 || i===130,down:i>=130&&i<145})},
  {name:'thin-wall', frames:120, solids:[{x:170,y:152,w:4,h:300}], commands:i=>({move:1,dashPressed:i===12})},
  {name:'ceiling', frames:120, solids:[{x:0,y:357,w:600,h:20}], commands:i=>({jumpPressed:i===12})},
  {name:'camera-travel', frames:480, commands:i=>({move:1,jumpPressed:i===180,jumpReleased:i===198})}
];
const cases=definitions.map(d=>{
  const {game,down,up}=boot(); game.startRun();
  Object.assign(game.world,{width:3200,top:-660,bottom:540,ground:[{x:0,y:452,w:3200,h:100}],solids:d.solids||[],platforms:d.platforms||[],hazards:[]});
  game.enemies.length=0;
  Object.assign(game.player,{x:110,y:412,vx:0,vy:0,grounded:true,inv:100});
  const samples=[];
  for(let i=0;i<d.frames;i++){
    const c=d.commands(i);
    game.keys.delete('a');game.keys.delete('d');game.keys.delete('s');
    if(c.move) game.keys.add(c.move>0?'d':'a');
    if(c.down) game.keys.add('s');
    if(c.jumpPressed){up(' ');down(' ');}
    if(c.jumpReleased) up(' ');
    if(c.dashPressed) game.pressed.add('k');
    game.update(1/120);
    const p=game.player;
    samples.push({command:{move:c.move||0,down:!!c.down,jumpPressed:!!c.jumpPressed,jumpReleased:!!c.jumpReleased,dashPressed:!!c.dashPressed},x:(p.x+11)/100,y:(452-p.y-p.h)/100,vx:p.vx/100,vy:-p.vy/100,grounded:p.grounded,airDashUsed:p.airDashUsed,cameraX:game.camera/100+4.8,cameraY:(452-game.cameraY)/100-2.7});
  }
  const box=r=>({x:r.x/100,y:(452-r.y-r.h)/100,width:r.w/100,height:r.h/100});
  return {name:d.name,solids:(d.solids||[]).map(box),platforms:(d.platforms||[]).map(box),samples};
});
const output={source:'game.js',sha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(source,'game.js'))).digest('hex'),step:1/120,cases};
const out=path.join(__dirname,'../Docs/Validation/movement-reference.json');
fs.writeFileSync(out,JSON.stringify(output,null,2)+'\n');
console.log(`Captured ${cases.length} original trajectories (${cases.reduce((n,c)=>n+c.samples.length,0)} ticks) to ${out}`);
