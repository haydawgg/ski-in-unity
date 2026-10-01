"""Build concise contact/line receipts from 100 Hz CSV and captured frames."""
from pathlib import Path
import csv,json,math,statistics,subprocess
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'DevelopmentCaptures/ContactLinePass'

def rows(path):
    return [{k:float(v) for k,v in row.items()} for row in csv.DictReader(path.open())]
def metrics(path):
    data=rows(path);sample=data[100:];ground=[r for r in sample if r['grounded']]
    if not ground:ground=sample
    def sd(key):return statistics.pstdev(r[key] for r in ground) if len(ground)>1 else 0
    def step(key):return statistics.mean(abs(b[key]-a[key]) for a,b in zip(ground,ground[1:])) if len(ground)>1 else 0
    return {'ticks':len(data),'groundedTicks':sum(r['grounded'] for r in data),'transitions':int(max(r['transitions'] for r in data)),
            'oneSkiTicks':sum(r['left']!=r['right'] for r in data),'heightSDmm':sd('height')*1000,'heightStepMm':step('height')*1000,
            'normalVelocitySD':sd('normalVelocity'),'supportAccelerationSD':sd('supportAcceleration'),'finalSpeed':data[-1]['speed']}
summary={}
for before in sorted((OUT/'Before/Telemetry').glob('*.csv')):
    after=OUT/'After/Telemetry'/before.name
    if after.exists():summary[before.stem]={'before':metrics(before),'after':metrics(after)}
comparison=OUT/'Comparison';comparison.mkdir(exist_ok=True)
(comparison/'stability-summary.json').write_text(json.dumps(summary,indent=2)+'\n')
with (comparison/'stability-summary.csv').open('w') as file:
    writer=csv.writer(file);writer.writerow(['route','before_height_sd_mm','after_height_sd_mm','before_support_sd','after_support_sd','before_transitions','after_transitions'])
    for name,data in summary.items():
        a,b=data['before'],data['after'];writer.writerow([name,a['heightSDmm'],b['heightSDmm'],a['supportAccelerationSD'],b['supportAccelerationSD'],a['transitions'],b['transitions']])
fig,axes=plt.subplots(3,2,figsize=(13,9),layout='constrained')
for column,name in enumerate(['Slope','ProductionExistingParkLine']):
    for label,color in [('Before','#ce6342'),('After','#087f8c')]:
        path=OUT/label/'Telemetry'/f'{name}.csv'
        if not path.exists():continue
        data=rows(path);t=[r['time'] for r in data]
        for axis,key,title in [(axes[0,column],'height','Support height at valid contacts (m)'),(axes[1,column],'normalVelocity','Velocity along support normal (m/s)'),(axes[2,column],'grounded','Grounded (0/1)')]:
            axis.plot(t,[r[key] if key!='height' or r['grounded'] else math.nan for r in data],label=label,color=color,linewidth=1.1);axis.set_ylabel(title);axis.grid(alpha=.2)
    axes[0,column].set_title('Constant slope / four sweeps' if column==0 else 'Same existing two-jump park route')
    axes[2,column].set_xlabel('Simulation time (s)');axes[0,column].legend()
fig.suptitle('Contact quality: identical controls, 100 Hz sampling; initial settling retained',fontsize=14)
fig.savefig(comparison/'ContactStability.png',dpi=140);plt.close(fig)

# MP4s are compact review artifacts; the raw PNG sequences stay local.
for phase in ['Before','After']:
    for sequence in sorted((OUT/phase/'Sequences').glob('*')):
        if not sequence.is_dir() or not list(sequence.glob('*.png')):continue
        dest=OUT/phase/'Videos'/f'{sequence.name}.mp4';dest.parent.mkdir(exist_ok=True)
        rate=10 if (sequence.name in ['AuthoredLine','AlternateLine'] or sequence.name.startswith('Lab')) else 6 if sequence.name=='ContinuousParkLine' else 5
        subprocess.run(['ffmpeg','-v','error','-y','-framerate',str(rate),'-i',str(sequence/'%04d.png'),'-c:v','libx264','-preset','fast','-crf','22','-pix_fmt','yuv420p',str(dest)],check=True)

font=ImageFont.truetype('/usr/share/fonts/TTF/DejaVuSans.ttf',17)
for name in ['AuthoredLine','AlternateLine']:
    events=OUT/'After'/f'{name}-events.csv'
    if not events.exists():continue
    all_events=list(csv.DictReader(events.open()));chosen=[e for e in all_events if e['event'] in ['takeoff','landing'] or e['event'].startswith('rail:')]
    chosen=chosen[:12]
    cols=3;cell_w=480;cell_h=300;sheet=Image.new('RGB',(cols*cell_w,math.ceil(len(chosen)/cols)*cell_h),(22,28,36));draw=ImageDraw.Draw(sheet)
    for i,e in enumerate(chosen):
        index=max(0,round((float(e['time'])+.15)*10));frames=sorted((OUT/'After/Sequences'/name).glob('*.png'))
        if not frames:continue
        frame=frames[min(index,len(frames)-1)];x=i%cols*cell_w;y=i//cols*cell_h
        with Image.open(frame) as image:sheet.paste(image.resize((cell_w,270)),(x,y))
        draw.text((x+9,y+276),f"{float(e['time']):.1f}s  {e['event']}  z={float(e['z']):.0f}",font=font,fill=(240,242,244))
    sheet.save(comparison/f'{name}Review.jpg',quality=90)
print('CONTACT / LINE COMPARISON PASS',len(summary),'matched routes')
