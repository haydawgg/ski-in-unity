"""Run both authored branches in the existing Linux development player."""
from pathlib import Path
import csv,json,shutil,subprocess
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'DevelopmentCaptures/ContactLinePass/After'
OUT.mkdir(parents=True,exist_ok=True)
results={}
for name,extra in [('NativeAuthoredLine',[]),('NativeAlternateLine',['--line-branch','--smoke-speed=22'])]:
    player=ROOT/'Builds/Linux';log=ROOT/'Logs'/f'{name}.log'
    for filename in ['line-events.csv','line-performance.json','line-exit.png']:(player/filename).unlink(missing_ok=True)
    command=['timeout','-k','5','60',str(ROOT/'Tools/Build/run.sh'),'--smoke-test','--smoke-line','--profile','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-logFile',str(log),*extra]
    run=subprocess.run(command,cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
    if run.returncode:raise RuntimeError(f'{name}: exit {run.returncode}; inspect {log}\n{run.stdout[-1000:]}')
    text=log.read_text()
    for marker in ['Exception:','Shader error','NullReferenceException']:assert marker not in text,(name,marker)
    events=list(csv.DictReader((player/'line-events.csv').open()))
    assert events[-1]['event']=='exit' and float(events[-1]['z'])<800 and not any(e['event']=='reset' for e in events)
    required={'rail:'+('Box_wide' if extra else 'Rail_down'),'rail:Box_down','rail:Rail_wide'}
    assert required.issubset({e['event'] for e in events})
    assert any(e['event']=='rail:'+('Box_wide' if extra else 'Rail_down') for e in events)
    assert sum(e['event']=='takeoff' for e in events)>=2
    assert sum(e['event']=='landing' for e in events)>=2
    performance=json.loads((player/'line-performance.json').read_text())
    assert performance['frameRateCap']==144 and (performance['width'],performance['height'],performance['quality'])==(1920,1080,2)
    results[name]={'exitCode':run.returncode,'seconds':float(events[-1]['time']),'launches':sum(e['event']=='takeoff' for e in events),'landings':sum(e['event']=='landing' for e in events),'railCaptures':sum(e['event'].startswith('rail:') for e in events),'performance':performance}
    for source,target in [('line-events.csv',name+'-events.csv'),('line-performance.json',name+'-performance.json'),('line-exit.png',name+'Exit.png')]:shutil.copy2(player/source,OUT/target)
    print(f"{name}: {results[name]['seconds']:.2f}s, {performance['averageFps']:.2f} FPS, p95 {performance['p95FrameMs']:.2f}ms, exit 0",flush=True)
(OUT/'native-line-runs.json').write_text(json.dumps(results,indent=2)+'\n')
