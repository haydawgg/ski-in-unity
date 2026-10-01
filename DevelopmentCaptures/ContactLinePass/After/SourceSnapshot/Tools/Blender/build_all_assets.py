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
if stage=='backdrop':
    from create_environment import ridge
    new.extend(ridge(i) for i in range(5))
if stage=='park':
    from create_park import build_park
    new.extend(build_park())
if stage=='jumps':
    from create_park import build_jumps
    new.extend(build_jumps())
if stage in ('all','park','line'):
    from create_park import build_line_assets
    new.extend(build_line_assets())
replacements={r['name']:r for r in new};existing={r['name'] for r in records}
records=[replacements.get(r['name'],r) for r in records]+[r for r in new if r['name'] not in existing]
manifest.write_text(json.dumps({'seed':42,'assets':records},indent=2))
print('ASSET BUILD PASS',len(new),'assets',sum(r['triangleCount'] for r in new),'triangles')
