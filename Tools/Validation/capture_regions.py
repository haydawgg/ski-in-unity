"""Capture selected Linux gameplay views with optional native performance measurements."""
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
parser.add_argument('--output', default='Documentation/VisualPolish/Phase7')
parser.add_argument('--only', nargs='+', choices=[name for name, _, _ in scenarios])
parser.add_argument('--rail', action='store_true', help='Review an actual rail capture/pop/landing before the Park run')
parser.add_argument('--profile', action='store_true', help='Opt in to native frame-time/CPU/allocation sampling after warmup')
args = parser.parse_args()
selected = args.only or ['SunsetEasy']
if args.rail and selected != ['SunsetPark']:
    parser.error('--rail requires --only SunsetPark')
output = ROOT / args.output
output.mkdir(parents=True, exist_ok=True)
results = json.loads((output / "native-runs.json").read_text()) if (output / "native-runs.json").exists() else {}
for name, area, day in scenarios:
    if name not in selected:
        continue
    log = ROOT / 'Logs' / ('visual-native-' + name + '.log')
    player_dir = ROOT / 'Builds/Linux'
    filenames = ['smoke-report.json', 'smoke-gameplay.png']
    if args.profile:
        filenames.append('performance-report.json')
    if args.rail:
        filenames += ['smoke-rail.png', 'smoke-rail-exit.png', 'smoke-rail-landing.png', 'smoke-rail-report.json']
    for filename in filenames:
        (player_dir / filename).unlink(missing_ok=True)
    command = ['timeout', '-k', '5', '45', str(ROOT / 'Tools/Build/run.sh'),
               '--smoke-test', '--smoke-area=' + str(area),
               '-screen-width', '1920', '-screen-height', '1080',
               '-screen-fullscreen', '0', '-logFile', str(log)]
    if day:
        command.append('--visual-day')
    if args.rail:
        command.append('--smoke-rail')
    if args.profile:
        command.append('--profile')
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
    if args.profile:
        performance = json.loads((player_dir / 'performance-report.json').read_text())
        assert performance['samples'] > 100 and performance['meanFrameMs'] > 0
        shutil.copy2(player_dir / 'performance-report.json', output / (name + '-performance.json'))
        print(f"{name}: observed {performance['averageFps']:.1f} FPS; p95 {performance['p95FrameMs']:.2f} ms; {performance['gpu']}", flush=True)
    if args.rail:
        rail = json.loads((player_dir / 'smoke-rail-report.json').read_text())
        assert rail['captured'] and rail['landed'] and rail['popSpeed'] > 8
        assert rail['leftAxis'] > .98 and rail['rightAxis'] > .98
        for source, target in [('smoke-rail.png', 'NativeRail.png'), ('smoke-rail-exit.png', 'NativeRailExit.png'), ('smoke-rail-landing.png', 'NativeRailLanding.png'), ('smoke-rail-report.json', 'native-rail.json')]:
            shutil.copy2(player_dir / source, output / target)
    print(f"{name}: 144 FPS cap; {report['distance']:.2f} m descent; capture saved; exit 0", flush=True)
(output / 'native-runs.json').write_text(json.dumps(results, indent=2) + '\n')
