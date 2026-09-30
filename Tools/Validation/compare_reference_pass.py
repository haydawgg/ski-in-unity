"""Build matched scenario comparisons and encode the recorded 12 fps sequences."""
import argparse,json,subprocess
from pathlib import Path
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--before',default='DevelopmentCaptures/BeforeReferencePass')
parser.add_argument('--after',default='DevelopmentCaptures/AfterReferencePass')
parser.add_argument('--output',default='DevelopmentCaptures/ReferenceComparison')
parser.add_argument('--encode-only',help='Encode/review one capture directory without comparing')
args=parser.parse_args()

def encode(folder):
    videos=folder/'Videos';videos.mkdir(exist_ok=True)
    for sequence in sorted((folder/'Sequences').iterdir()):
        frames=sorted(sequence.glob('*.png'))
        if not frames:continue
        target=videos/(sequence.name+'.mp4')
        if not target.exists() or target.stat().st_mtime<max(p.stat().st_mtime for p in frames):
            subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-framerate',('6' if sequence.name=='ContinuousParkLine' else '12'),'-i',str(sequence/'%04d.png'),'-c:v','libx264','-threads','2','-crf','19','-pix_fmt','yuv420p',str(target)],check=True)
    labels=['Straight','CarveLeft','CarveRight','HardSkid','SmallJumpAir','LargeJumpAir','Spin360','Spin720','Flip','Cork','Grab','SketchyLanding','RailEntry','RailSlide','RailExit','RailExitLanding','ParkRun','MountainSunset']
    w,h=480,270;sheet=Image.new('RGB',(w*3,(h+26)*6),(19,23,29));draw=ImageDraw.Draw(sheet)
    for i,label in enumerate(labels):
        picture=Image.open(folder/'States'/(label+'.png')).convert('RGB');picture.thumbnail((w,h));x=i%3*w;y=i//3*(h+26);sheet.paste(picture,(x,y+26));draw.text((x+8,y+8),label,fill='white')
    sheet.save(folder/'ScenarioReview.jpg',quality=91)

if args.encode_only:
    encode(ROOT/args.encode_only);raise SystemExit

before=ROOT/args.before;after=ROOT/args.after;output=ROOT/args.output;output.mkdir(parents=True,exist_ok=True)
for folder in [before,after]:encode(folder)
old={m['name']:m for m in json.loads((before/'scenario-metrics.json').read_text())['samples']}
new={m['name']:m for m in json.loads((after/'scenario-metrics.json').read_text())['samples']}
groups={
    'CameraCarve':[('Straight',0),('CarveLeft',38),('CarveRight',38),('HardSkid',40)],
    'AirGrabLanding':[('SmallJumpAir',1),('Spin720',23),('Grab',32),('SmallJumpLanding',33),('SketchyLanding',40)],
    'RailEnvironment':[('RailSlide',58),('RailExit',60),('RailExitLanding',61),('MountainSunset',21),('MountainDay',57)]}
for group,rows in groups.items():
    w,h=480,270;refw=152;band=34;sheet=Image.new('RGB',(w*2+refw,(h+band)*len(rows)),(19,23,29));draw=ImageDraw.Draw(sheet)
    for row,(label,second) in enumerate(rows):
        y=row*(h+band)
        for x,folder,title in [(0,before,'Before'),(w+refw,after,'After')]:
            picture=Image.open(folder/'States'/(label+'.png')).convert('RGB');picture.thumbnail((w,h));sheet.paste(picture,(x,y+band));draw.text((x+8,y+8),title+' / '+label,fill='white')
        picture=Image.open(ROOT/'Reference/Analysis'/f'frame-{second+1:03d}.jpg').convert('RGB');picture.thumbnail((refw,h));sheet.paste(picture,(w,y+band));draw.text((w+4,y+8),f'Ref {second+.5}s',fill='white')
    sheet.save(output/(group+'.jpg'),quality=93)
lines=['# Matched before/after measurements','','Head-to-boot height is a skeleton screen-span measure; portrait reference estimates include equipment and are not identical measures. Radius is speed squared divided by measured lateral load, an estimate during the sampled turn. Named scenarios relocate explicitly; each recorded sequence is continuous. Full rotations use seeded angular momentum, and the rail pop uses input-only feedback to reach switch. Landing labels outside contact/result states are the most recent grade; the old baseline retained that grade across relocation.','','| State | Screen height % before / after | Speed m/s before / after | Bend before / after | Landing before / after |','| --- | --- | --- | --- | --- |']
comparison=[]
for label in old:
    if label not in new:continue
    a,b=old[label],new[label]
    lines.append(f"| {label} | {a['bodyScreenHeight']*100:.1f} / {b['bodyScreenHeight']*100:.1f} | {a['speed']:.2f} / {b['speed']:.2f} | {a['bend']:.3f} / {b['bend']:.3f} | {a['landing']} / {b['landing']} |")
    comparison.append({'scenario':label,'before':a,'after':b})
(output/'measurements.md').write_text('\n'.join(lines)+'\n')
(output/'measurements.json').write_text(json.dumps(comparison,indent=2)+'\n')
print('Comparison sheets, metrics and sequence videos written to',output)
