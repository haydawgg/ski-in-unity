"""Review native menu/HUD pages at desktop landscape sizes, without timing performance."""
import argparse
import json
import subprocess
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', default='Documentation/VisualPolish/Phase7/UI')
args = parser.parse_args()
OUTPUT = ROOT / args.output
LOG = ROOT / 'Logs/presentation-native-ui.log'
OUTPUT.mkdir(parents=True, exist_ok=True)
result = subprocess.run(['timeout', '-k', '5', '90', str(ROOT / 'Tools/Build/run.sh'),
                         '--presentation-review', '--presentation-output=' + str(OUTPUT),
                         '-screen-fullscreen', '0', '-logFile', str(LOG)],
                        cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
if result.returncode:
    raise RuntimeError(f'UI review exit {result.returncode}; inspect {LOG}\n{result.stdout[-1200:]}')
text = LOG.read_text()
for marker in ['Exception:', 'NullReferenceException', 'Shader error', 'ERROR:']:
    assert marker not in text, (marker, LOG)
report = json.loads((OUTPUT / 'native-ui.json').read_text())
assert report['frameRateCap'] == 144
assert report['pointerAdjustment'] and report['pointerSave']
assert len(report['captures']) == 18
for capture in report['captures']:
    image = Image.open(OUTPUT / (capture['name'] + '.png'))
    assert image.size == (capture['width'], capture['height']), capture
assert set((c['width'], c['height']) for c in report['captures']) == {(1280, 720), (1920, 1080)}
report['exitCode'] = result.returncode
(OUTPUT / 'native-ui.json').write_text(json.dumps(report, indent=2) + '\n')
print('Native UI: 18 captures / two desktop landscape sizes; pointer adjustment/save pass; cap 144; exit 0')
