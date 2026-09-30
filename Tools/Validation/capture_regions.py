"""Capture selected Linux gameplay views at 144 FPS without performance measurements."""
import argparse
import json
import shutil
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
scenarios = [('SunsetEasy', 0, False), ('SunsetPark', 1, False),
             ('SunsetBigAir', 2, False), ('SunsetFreeride', 3, False),
             ('SunsetLower', 4, False), ('DayEasy', 0, True), ('DayFreeride', 3, True)]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', default='Documentation/VisualPolish/Phase3')
parser.add_argument('--only', nargs='+', choices=[name for name, _, _ in scenarios])
args = parser.parse_args()
selected = args.only or ['SunsetEasy']
output = ROOT / args.output
output.mkdir(parents=True, exist_ok=True)
results = {}
for name, area, day in scenarios:
    if name not in selected:
        continue
    log = ROOT / 'Logs' / ('visual-native-' + name + '.log')
    player_dir = ROOT / 'Builds/Linux'
    for filename in ['smoke-report.json', 'smoke-gameplay.png']:
        (player_dir / filename).unlink(missing_ok=True)
    command = ['timeout', '-k', '5', '45', str(ROOT / 'Tools/Build/run.sh'),
               '--smoke-test', '--smoke-area=' + str(area),
               '-screen-width', '1920', '-screen-height', '1080',
               '-screen-fullscreen', '0', '-logFile', str(log)]
    if day:
        command.append('--visual-day')
    if area == 3:
        command.append('--smoke-speed=15')
    if area == 4:
        command.append('--smoke-duration=8')
    launch = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    if launch.returncode:
        raise RuntimeError(f'{name}: player exit {launch.returncode}; inspect {log}\n{launch.stdout[-2000:]}')
    text = log.read_text()
    for marker in ['Exception:', 'NullReferenceException', 'Shader error', 'ERROR:']:
        assert marker not in text, (name, marker, log)
    report = json.loads((player_dir / 'smoke-report.json').read_text())
    assert report['frameRateCap'] == 144 and report['vSyncCount'] == 0
    assert report['distance'] > 30
    assert (report['width'], report['height'], report['quality']) == (1920, 1080, 2)
    assert report['day'] == day
    report['exitCode'] = launch.returncode
    results[name] = report
    (output / (name + '-run.json')).write_text(json.dumps(report, indent=2) + '\n')
    shutil.copy2(player_dir / 'smoke-gameplay.png', output / (name + 'Player.png'))
    print(f"{name}: 144 FPS cap; {report['distance']:.2f} m descent; capture saved; exit 0", flush=True)
(output / 'native-runs.json').write_text(json.dumps(results, indent=2) + '\n')
