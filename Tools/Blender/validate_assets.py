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
        if a['type'] in ('Trees','EnvironmentAccent') and 'lodTriangles' in a['parameters']:
            lods=a['parameters']['lodTriangles'];assert len(lods)==3 and lods[0]>lods[1]>lods[2]>0
            assert sum(lods)==aTris and lods[0]<6000
        if a['type']=='EnvironmentAccent':assert not a['collisionAsset'] and a['parameters']['collision'] is False
    print('MANIFEST VALIDATION PASS',len(names),'assets')
if __name__=='__main__':validate()
