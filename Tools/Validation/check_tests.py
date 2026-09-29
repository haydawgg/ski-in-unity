import sys
import xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
print(f"Tests: {root.get('total')}, passed: {root.get('passed')}, failed: {root.get('failed')}, result: {root.get('result')}")
assert int(root.get('total', '0')) > 0, 'No tests were run'
assert root.get('result') == 'Passed', 'Tests failed; see XML report'
