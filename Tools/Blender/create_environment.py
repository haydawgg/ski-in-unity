"""Seeded alpine terrain, terrain park, LOD pines, rocks and mountain props."""
from common.pipeline import *

def terrain_height(x,z):
    central=math.exp(-(x/48)**4)
    edge=(abs(x)/200)**3*95
    ripple=(math.sin(z*.022+x*.023)*3+math.sin(z*.07-x*.02)*1.5)*(1-central*.82)
    freeride=math.sin(z*.046)*4*max(0,min(1,(x-50)/55))
    return 355-.23*z+edge+ripple+freeride

def snow_material():return material('snow',(.81,.88,.96))
def tree(variant):
    clear();height=[5,8,12,7][variant];green=material('pine_dark',(.045,.10,.105));snow=snow_material();bark=material('bark',(.13,.10,.095))
    for lod,(sides,tiers) in enumerate([(12,8),(8,5),(5,3)]):
        rod(f'Tree_LOD{lod}_trunk',(0,0,0),(0,height,0),height*.026,bark,sides)
        for i in range(tiers):
            y=height*(.14+i*.74/tiers);radius=height*(.20-.14*i/tiers);depth=height*.33
            for cap in [False,True]:
                bpy.ops.mesh.primitive_cone_add(vertices=sides,radius1=radius*(.96 if cap else 1),radius2=0,depth=depth*(.86 if cap else 1),location=v((0,y+depth*.5+(depth*.085 if cap else 0),0)))
                o=bpy.context.object;o.name=f'Tree_LOD{lod}_{i}_'+('snow' if cap else 'needles');o.data.materials.append(snow if cap else green)
    return export(f'Pine{variant}','Trees',{'seed':42+variant,'height':height,'lodCount':3},variant==2)
def rock(variant):
    clear();random.seed(420+variant);stone=material('rock',(.13,.15,.22));snow=snow_material();o=ellipsoid('Rock',(.0,.8,0),(1.5+variant*.15,1+variant*.1,1.2),stone,12,8);o.data.materials.append(snow)
    for vert in o.data.vertices:vert.co*=random.uniform(.8,1.2)
    for face in o.data.polygons:
        if face.center.z>.2 or face.normal.z>.5:face.material_index=1
    return export(f'Rock{variant}','Rocks',{'seed':420+variant},variant==0)
def rail(kind):
    clear();steel=material('steel',(.028,.035,.05),.3);length=12;w=.13
    if kind=='kink':points=[(0,1.2,0),(0,.3,6),(0,-1.4,12)]
    elif kind=='rainbow':points=[(0,1.2+1.8*math.sin(math.pi*i/24),length*i/24) for i in range(25)]
    elif kind=='down':points=[(0,1.2,0),(0,-1.5,length)]
    else:points=[(0,1.2,0),(0,1.2,length)]
    for i in range(1,len(points)):rod('BlackTube_'+str(i),points[i-1],points[i],.07 if kind!='wide' else .13,steel,12)
    for z in [1,4,8,11]:
        y=1.2 if kind=='flat' else 1.2-.225*z
        rod('Support'+str(z),(0,-.23*z,z),(0,y,z),.04,steel,8);box('Foot'+str(z),(0,-.23*z,z),(.6,.08,.4),steel)
    metadata={'railId':kind.title(),'width':w,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]}
    return export('Rail_'+kind,'Rails',{'length':length,'height':1.2,'tubeRadius':.07,'seed':42},kind=='kink',metadata)
def grind_box(kind):
    clear();steel=material('steel',(.035,.04,.055));top=material('box_top',(.20,.34,.39),.35);length=10;width=1.8 if kind=='wide' else .65;points=[]
    for i in range(21):
        z=length*i/20;y=.8-(.23*z if kind=='down' else max(0,z-5)*.3 if kind=='kink' else 0);points.append((0,y,z))
    vertices=[]
    for x,y,z in points:vertices.extend([(-width*.5,y,z),(width*.5,y,z),(-width*.5,y-.5,z),(width*.5,y-.5,z)])
    faces=[]
    for i in range(20):a=i*4;faces.extend([(a,a+4,a+5,a+1),(a,a+2,a+6,a+4),(a+1,a+5,a+7,a+3)])
    mesh('BoxRidingSurface',vertices,faces,top)
    return export('Box_'+kind,'Boxes',{'width':width,'length':length},kind=='wide',{'railId':kind.title()+' Box','width':width,'points':[{'x':x,'y':y,'z':z} for x,y,z in points]})
def jump(name,width,length,height,deck=0):
    clear();mat=snow_material();vertices=[];faces=[];sections=96;total=length+deck+(length*1.5 if deck else 0)
    for i in range(sections+1):
        z=total*i/sections
        if z<=length:y=-.23*z+height*(z/length)**2
        elif z<=length+deck:y=-.23*z+height
        else:
            t=(z-length-deck)/(total-length-deck);y=-.23*z+height*(1-t*t*(3-2*t))
        for x in [-width*.5,width*.5]:vertices.append((x,y,z))
    for i in range(sections):a=i*2;faces.append((a,a+2,a+3,a+1))
    mesh('ContinuousSnowSurface',vertices,faces,mat)
    # Sides are closed down to the terrain; no staircase riding collision.
    sideverts=[];sidefaces=[]
    for side in [0,1]:
        start=len(sideverts)
        for i in range(sections+1):x,y,z=vertices[i*2+side];sideverts.extend([(x,y,z),(x,-.23*z-.1,z)])
        for i in range(sections):a=start+i*2;sidefaces.append((a,a+1,a+3,a+2))
    mesh('SnowSides',sideverts,sidefaces,mat)
    return export(name,'Jumps',{'width':width,'length':length,'height':height,'deck':deck,'continuousCollision':True},name=='JumpMedium')
def mountain(chunk):
    clear();mat=snow_material();vertices=[];faces=[];nx=80;nz=30
    for j in range(nz+1):
        z=chunk*150+j*5
        for i in range(nx+1):x=-200+i*5;vertices.append((x,terrain_height(x,z),z))
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+nx+1,a+nx+2,a+1))
    o=mesh('SmoothMountainChunk',vertices,faces,mat)
    for p in o.data.polygons:p.use_smooth=True
    return export(f'Mountain{chunk:02}','Mountain',{'chunk':chunk,'length':150,'width':400,'spacing':5,'seed':42})
def ridge(variant):
    clear();random.seed(440+variant);mat=material('distant_rock',(.19,.22,.34));snow=snow_material();verts=[];faces=[];n=100
    for j in range(13):
        r=j/12
        for i in range(n+1):
            x=-700+1400*i/n;peak=220+100*math.sin(i*.19+variant)**2+70*math.sin(i*.43+variant)**2
            y=40+peak*math.sin(r*math.pi)**1.8;z=r*450
            verts.append((x,y,z))
    for j in range(12):
        for i in range(n):a=j*(n+1)+i;faces.append((a,a+n+1,a+n+2,a+1))
    o=mesh('AlpineRidge',verts,faces,mat);o.data.materials.append(snow)
    for p in o.data.polygons:
        p.use_smooth=True
        if p.center.z>180 and p.normal.z>.35:p.material_index=1
    return export(f'Ridge{variant}','EnvironmentDistant',{'seed':440+variant})
def prop(kind):
    clear();steel=material('steel',(.04,.045,.065));wood=material('wood',(.26,.14,.09));snow=snow_material();light=material('lamp',(.98,.87,.65))
    if kind=='Hut':
        box('RidgeLodge',(0,1.5,0),(7,3,5),wood,.05);box('SnowRoof',(0,3.1,0),(8,.28,6),snow,.05);box('Door',(0,1,.0+2.52),(1.3,2,.04),steel)
        for x in [-2,2]:box('Window'+str(x),(x,1.8,2.52),(1.1,.9,.04),light)
    elif kind=='LiftTower':
        rod('Tower',(0,0,0),(0,14,0),.24,steel);rod('CrossArm',(-4,14,0),(4,14,0),.2,steel)
        for x in [-3,3]:box('CableWheel'+str(x),(x,14.2,0),(.5,.4,.5),steel,.08)
    elif kind=='Floodlight':
        rod('Mast',(0,0,0),(0,10,0),.1,steel);box('LampHousing',(0,10,0),(1.2,.4,.5),steel,.04);box('LampLens',(0,9.95,.26),(1,.25,.04),light)
    elif kind=='Fence':
        for z in [0,2,4]:rod('Post'+str(z),(0,0,z),(0,1.2,z),.06,wood)
        for y in [.4,1]:rod('CrossRail'+str(y),(0,y,0),(0,y,4),.035,wood)
    elif kind=='Sign':
        rod('Signpost',(0,0,0),(0,2.3,0),.07,wood);box('DirectionBoard',(0,2,0),(2.5,.6,.14),wood,.06)
    elif kind=='Flag':
        rod('FlagPole',(0,0,0),(0,2.5,0),.035,steel);box('Flag',(0.5,2.1,0),(1,.55,.025),material('flag',(.93,.35,.22)))
    elif kind=='LiftCable':rod('Cable',(0,0,0),(0,0,100),.025,steel,6)
    return export(kind,'Environment',{'seed':42},kind=='Hut')
def build_environment():
    records=[]
    for i in range(4):records.append(tree(i))
    for i in range(6):records.append(rock(i))
    for kind in ['flat','down','kink','rainbow','wide']:records.append(rail(kind))
    for kind in ['flat','down','wide','narrow','kink']:records.append(grind_box(kind))
    for args in [('JumpSmall',6,10,1.8,0),('JumpMedium',10,16,4.5,0),('JumpLarge',18,24,8,0),('Tabletop',18,18,5,12),('Hip',12,15,4,0),('QuarterPipe',16,7,6,0)]:records.append(jump(*args))
    for i in range(10):records.append(mountain(i))
    for i in range(3):records.append(ridge(i))
    clear();vertices=[];faces=[]
    for j in range(21):
        z=1500+j*40
        for i in range(31):x=-750+i*50;vertices.append((x,10-.005*(z-1500)+(abs(x)/750)**2*16,z))
    for j in range(20):
        for i in range(30):a=j*31+i;faces.append((a,a+31,a+32,a+1))
    mesh('LowerValleyBackdrop',vertices,faces,snow_material());records.append(export('ValleyFloor','EnvironmentDistant',{'seed':42,'collision':False}))
    for name in ['Hut','LiftTower','Floodlight','Fence','Sign','Flag','LiftCable']:records.append(prop(name))
    return records
if __name__=='__main__':build_environment()
