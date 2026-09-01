"""Generira chiptune glazbu i zvucne efekte za Run, Slavko, Run!
Sve je sintetizirano, bez vanjskih uzoraka, pa nema pitanja licence.
Izlaz: 16-bitni mono WAV, 44100 Hz.
"""
import numpy as np, wave, os, math

SR = 44100
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz', 'audio')
os.makedirs(OUT, exist_ok=True)

# poluton -> frekvencija (A4 = 440 Hz)
def hz(note):
    return 440.0 * 2 ** ((note - 69) / 12.0)

NOTE = {'C': 0, 'C#': 1, 'D': 2, 'D#': 3, 'E': 4, 'F': 5, 'F#': 6,
        'G': 7, 'G#': 8, 'A': 9, 'A#': 10, 'B': 11}

def n(name, octave):
    return 12 * (octave + 1) + NOTE[name]


def square(freq, dur, duty=0.5, vol=0.25, attack=0.004, release=0.05):
    t = np.arange(int(SR * dur)) / SR
    ph = (t * freq) % 1.0
    w = np.where(ph < duty, 1.0, -1.0)
    env = np.ones_like(w)
    a = max(1, int(SR * attack))
    r = max(1, int(SR * release))
    env[:a] = np.linspace(0, 1, a)
    env[-r:] = np.linspace(1, 0, r)
    # blagi pad glasnoce kroz ton
    env *= np.linspace(1.0, 0.75, len(w))
    return w * env * vol


def triangle(freq, dur, vol=0.3, release=0.04):
    t = np.arange(int(SR * dur)) / SR
    ph = (t * freq) % 1.0
    w = 4 * np.abs(ph - 0.5) - 1
    env = np.ones_like(w)
    a = max(1, int(SR * 0.004))
    r = max(1, int(SR * release))
    env[:a] = np.linspace(0, 1, a)
    env[-r:] = np.linspace(1, 0, r)
    return w * env * vol


def noise(dur, vol=0.18, decay=18.0):
    t = np.arange(int(SR * dur)) / SR
    rng = np.random.default_rng(7)
    w = rng.uniform(-1, 1, len(t))
    return w * np.exp(-decay * t) * vol


def place(buf, sig, start_s):
    i = int(SR * start_s)
    end = min(len(buf), i + len(sig))
    if i < len(buf):
        buf[i:end] += sig[:end - i]


def save(name, buf):
    peak = np.max(np.abs(buf))
    if peak > 0:
        buf = buf / peak * 0.85
    data = (buf * 32767).astype('<i2')
    with wave.open(f'{OUT}/{name}.wav', 'wb') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(SR)
        f.writeframes(data.tobytes())
    return len(buf) / SR


# ---------------------------------------------------------------- GLAZBA
def track(name, bpm, bars, chords, melody, duty=0.5, bass_oct=2,
          drums=True, mel_vol=0.22, swing=0.0):
    """chords: lista korijenskih nota po taktu. melody: lista (stupanj, trajanje u dobama)."""
    beat = 60.0 / bpm
    total = bars * 4 * beat
    buf = np.zeros(int(SR * total) + SR // 2)

    # bas: korijen na 1. i 3. dobu, kvinta na 4.
    for b in range(bars):
        root = chords[b % len(chords)]
        t0 = b * 4 * beat
        place(buf, triangle(hz(root - 12 * (3 - bass_oct)), beat * 0.9, 0.34), t0)
        place(buf, triangle(hz(root - 12 * (3 - bass_oct)), beat * 0.9, 0.30), t0 + 2 * beat)
        place(buf, triangle(hz(root - 12 * (3 - bass_oct) + 7), beat * 0.7, 0.26), t0 + 3 * beat)

    # melodija
    t = 0.0
    mi = 0
    while t < total - 1e-6:
        deg, dur = melody[mi % len(melody)]
        mi += 1
        if deg is not None:
            bar = int(t / (4 * beat))
            root = chords[bar % len(chords)]
            sw = swing if int(t / beat * 2) % 2 == 1 else 0.0
            place(buf, square(hz(root + deg), dur * beat * 0.92, duty, mel_vol), t + sw)
        t += dur * beat

    # bubnjevi: bas na 1 i 3, hi-hat na svaku osminu
    if drums:
        for b in range(bars):
            t0 = b * 4 * beat
            for k in (0, 2):
                place(buf, noise(0.10, 0.22, 42), t0 + k * beat)
            for k in range(8):
                place(buf, noise(0.035, 0.055, 120), t0 + k * beat * 0.5)

    dur = save(name, buf[:int(SR * total)])   # rez tocno na kraj takta -> bešavna petlja
    print(f'  {name:22s} {bpm:3d} BPM  {dur:5.1f} s')


C, D, E, F, G, A, B = (n('C', 4), n('D', 4), n('E', 4), n('F', 4),
                       n('G', 4), n('A', 4), n('B', 4))
Am, Dm, Em = n('A', 3), n('D', 4), n('E', 4)

print('glazba:')

# Izbornik — mirno, veselo, u C-duru
track('music_menu', 104, 8,
      [C, A - 12 + 12, F, G, C, A - 12 + 12, F, G],
      [(0, 1), (4, 0.5), (7, 0.5), (9, 1), (7, 1),
       (5, 1), (4, 0.5), (2, 0.5), (0, 1), (None, 1)],
      duty=0.5, mel_vol=0.20)

# Zoo — poskocno, brze, veselo
track('music_zoo', 132, 8,
      [C, F, G, C, C, F, G, C],
      [(0, 0.5), (4, 0.5), (7, 0.5), (12, 0.5), (9, 0.5), (7, 0.5), (4, 1),
       (2, 0.5), (5, 0.5), (9, 0.5), (7, 0.5), (4, 1)],
      duty=0.25, mel_vol=0.21, swing=0.012)

# Grad — brze, uporno, mol
track('music_city', 146, 8,
      [Am, Am, F, G, Am, Am, F, E],
      [(0, 0.5), (3, 0.5), (7, 0.5), (3, 0.5), (10, 0.5), (7, 0.5), (5, 0.5), (3, 0.5),
       (0, 0.5), (3, 0.5), (7, 1), (5, 0.5), (3, 0.5), (0, 1)],
      duty=0.125, mel_vol=0.19)

# Suma — sporije, tajanstveno
track('music_forest', 118, 8,
      [Em, Em, C, D, Em, Em, C, B - 12 + 12],
      [(0, 1), (3, 0.5), (7, 0.5), (10, 1), (7, 1),
       (5, 0.5), (3, 0.5), (2, 1), (0, 1), (None, 1)],
      duty=0.5, mel_vol=0.20)

# Tvornica — najbrze, ostro
track('music_factory', 160, 8,
      [Dm, Dm, Am, Dm, Dm, F, G, Am],
      [(0, 0.5), (0, 0.5), (5, 0.5), (7, 0.5), (10, 0.5), (7, 0.5), (5, 0.5), (3, 0.5),
       (0, 0.5), (7, 0.5), (12, 0.5), (10, 0.5), (7, 1), (0, 1)],
      duty=0.125, mel_vol=0.19)


# ---------------------------------------------------------------- EFEKTI
print('efekti:')

# skok — kratki uzlazni prijelaz
t = np.arange(int(SR * 0.16)) / SR
f = np.linspace(330, 760, len(t))
ph = np.cumsum(f) / SR
buf = np.where((ph % 1.0) < 0.4, 1.0, -1.0) * np.exp(-11 * t) * 0.5
print(f'  music_jump             {save("sfx_jump", buf):5.2f} s')

# pokupljeno — dva tona prema gore
buf = np.zeros(int(SR * 0.26))
place(buf, square(hz(n('E', 5)), 0.09, 0.5, 0.42, release=0.02), 0.0)
place(buf, square(hz(n('B', 5)), 0.16, 0.5, 0.40, release=0.06), 0.085)
print(f'  sfx_pickup             {save("sfx_pickup", buf):5.2f} s')

# udarac — sum i silazni ton
t = np.arange(int(SR * 0.28)) / SR
f = np.linspace(420, 90, len(t))
ph = np.cumsum(f) / SR
tone = np.where((ph % 1.0) < 0.5, 1.0, -1.0) * np.exp(-9 * t) * 0.42
buf = tone + noise(0.28, 0.30, 16)
print(f'  sfx_hit                {save("sfx_hit", buf):5.2f} s')

# kraj igre — silazni molski razlozeni akord
buf = np.zeros(int(SR * 1.15))
for i, note in enumerate([n('A', 4), n('F', 4), n('D', 4), n('A', 3)]):
    place(buf, square(hz(note), 0.34, 0.5, 0.34, release=0.12), i * 0.24)
print(f'  sfx_gameover           {save("sfx_gameover", buf):5.2f} s')

print()
print('ukupno datoteka:', len([f for f in os.listdir(OUT) if f.endswith(".wav")]))
print('ukupna velicina:', round(sum(os.path.getsize(f"{OUT}/{f}")
                                    for f in os.listdir(OUT)) / 1024 / 1024, 2), 'MB')
