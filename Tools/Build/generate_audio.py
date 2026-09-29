"""Original seeded procedural Foley; no music or downloaded samples."""
import math,random,wave,struct
from pathlib import Path
root=Path(__file__).resolve().parents[2];folder=root/'Assets/Audio';folder.mkdir(parents=True,exist_ok=True)
rate=22050;random.seed(42)
for name,seconds,frequency,decay in [('Snow',3,900,0),('Wind',3,200,0),('Rail',3,1900,0),('Landing',.3,85,15),('Crash',1,350,4),('Pop',.2,140,12)]:
    output=[];low=0;previous=0;count=int(rate*seconds)
    for i in range(count):
        t=i/rate;noise=random.uniform(-1,1);low=low*.92+noise*.08
        high=noise-previous;previous=noise
        sound=low*2 if name=='Wind' else high*.12+noise*.2 if name=='Snow' else noise*.2+math.sin(2*math.pi*frequency*t)*.08 if name=='Rail' else low*2+math.sin(2*math.pi*frequency*t)*.4
        env=math.exp(-decay*t) if decay else min(1,i/400,(count-i)/400)
        output.append(struct.pack('<h',int(max(-1,min(1,sound*env*.7))*32767)))
    with wave.open(str(folder/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(b''.join(output))
print('AUDIO GENERATION PASS: 6 original seeded WAVs')
