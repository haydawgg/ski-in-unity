import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def validate():
    manifest=json.loads((ROOT/'Assets/Art/Generated/asset_manifest.json').read_text())
    names=set()
    for a in manifest['assets']:
        assert a['name'] not in names;aPath=ROOT/a['fbxPath'];assert aPath.stat().st_size>1000;aTris=a['triangleCount'];assert 0<aTris<4000000;names.add(a['name'])
        if a['previewPath']:assert (ROOT/a['previewPath']).stat().st_size>0
        if a['type']=='Character':assert aTris>5000
    print('MANIFEST VALIDATION PASS',len(names),'assets')
if __name__=='__main__':validate()
