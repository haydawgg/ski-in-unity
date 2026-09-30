import sys,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from common.pipeline import ROOT,OUT
from create_skier import create_skier
stage=sys.argv[sys.argv.index('--stage')+1] if '--stage' in sys.argv else 'all'
manifest=OUT/'asset_manifest.json';OUT.mkdir(parents=True,exist_ok=True)
records=json.loads(manifest.read_text())['assets'] if manifest.exists() else []
new=[]
if stage in ('all','character'):new.append(create_skier())
if stage in ('all','environment'):
    from create_environment import build_environment
    new.extend(build_environment())
if stage=='scenery':
    from create_environment import build_scenery
    new.extend(build_scenery())
names={r['name'] for r in new};records=[r for r in records if r['name'] not in names]+new
manifest.write_text(json.dumps({'seed':42,'assets':records},indent=2))
print('ASSET BUILD PASS',len(new),'assets',sum(r['triangleCount'] for r in new),'triangles')
