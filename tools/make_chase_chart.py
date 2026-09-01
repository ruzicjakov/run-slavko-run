"""Dijagram potjere za zavrsni rad.

Simulira novu dinamiku (GuardAI + PlayerHealth.Stumble) korak po korak, istim
vremenskim korakom kao fizika u Unityju, i crta razmak cuvar-igrac kroz vrijeme
za tri scenarija: čista vožnja, jedno posrtanje i dva posrtanja u kratkom razmaku.
"""
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt

import os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'izlaz')
os.makedirs(OUT, exist_ok=True)

DT = 0.02
STOP = 0.9
REACH = 1.05
RECOIL = 0.7
INVULN = 1.5
KB_FWD = 4.0
KB_DUR = 0.35
NUDGE = -0.8
STUMBLE_DUR = 0.22

P = dict(run=6.0, base=5.6, catch=7.5, leash=3.0)
T_END = 26.0


def sim(stumbles, p=P, t_end=T_END, gap0=5.0):
    n = int(t_end / DT)
    px, gx = 0.0, -gap0
    recoil_until = invuln_until = suppress_until = stumble_cd = -1.0
    suppress_v = 0.0
    catches, applied = [], []
    ts, gaps = np.zeros(n), np.zeros(n)
    pending = list(stumbles)

    for i in range(n):
        now = i * DT
        if pending and now >= pending[0]:
            pending.pop(0)
            if now >= invuln_until and now >= stumble_cd:
                stumble_cd = now + 0.5
                suppress_until = now + STUMBLE_DUR
                suppress_v = NUDGE
                applied.append(now)

        px += (suppress_v if now < suppress_until else p['run']) * DT

        if now >= recoil_until:
            limit = px - STOP
            if gx >= limit:
                gx = limit
            else:
                d = px - gx
                gx = min(gx + (p['catch'] if d > p['leash'] else p['base']) * DT, limit)

        gap = px - gx
        if gap <= REACH and now >= invuln_until and now >= recoil_until:
            catches.append(now)
            invuln_until, recoil_until = now + INVULN, now + RECOIL
            suppress_until, suppress_v = now + KB_DUR, KB_FWD

        ts[i], gaps[i] = now, gap
    return ts, gaps, catches, applied


S1 = []
S2 = [10.0]
S3 = [10.0, 11.5]
t1, g1, c1, _ = sim(S1)
t2, g2, c2, a2 = sim(S2)
t3, g3, c3, a3 = sim(S3)
print('čista vožnja   : min %.2f j, hvatanja %d' % (g1[300:].min(), len(c1)))
print('jedno posrtanje: min %.2f j, hvatanja %d' % (g2[300:].min(), len(c2)))
print('dva posrtanja  : min %.2f j, hvatanja %d, u t = %s'
      % (g3[300:].min(), len(c3), [round(x, 2) for x in c3]))

INK, MUTED, GRID = '#1C212C', '#5A6270', '#DDE1E8'
BLUE, ORANGE, PURPLE, RED = '#2E6DA4', '#C25A1E', '#7A4E9E', '#B03030'

plt.rcParams['font.family'] = 'DejaVu Sans'
fig, ax = plt.subplots(figsize=(9.2, 4.6), dpi=200)
fig.patch.set_facecolor('#FCFCFB')
ax.set_facecolor('#FCFCFB')

ax.axhspan(0, REACH, color=RED, alpha=0.10, zorder=0)
ax.axhline(REACH, color=RED, lw=1.4, ls='--', zorder=2)
ax.text(0.3, REACH + 0.16, 'dohvat čuvara (1,05 j)', ha='left', va='bottom',
        fontsize=9, color=RED)

for m in S3:
    ax.axvspan(m, m + STUMBLE_DUR, color=MUTED, alpha=0.16, lw=0, zorder=1)

ax.plot(t3, g3, color=PURPLE, lw=2.0, zorder=4, label='dva posrtanja u 1,5 s')
ax.plot(t2, g2, color=ORANGE, lw=2.0, zorder=5, label='jedno posrtanje')
ax.plot(t1, g1, color=BLUE, lw=2.0, zorder=6, label='čista vožnja')

for x in c3:
    ax.plot([x], [REACH], 'o', ms=9, mfc=PURPLE, mec='#FCFCFB', mew=2, zorder=7)
    ax.annotate('čuvar hvata\n(gubitak života)', xy=(x, REACH), xytext=(4.6, 1.62),
                ha='left', fontsize=9.5, color=PURPLE, fontweight='bold',
                arrowprops=dict(arrowstyle='->', color=PURPLE, lw=1.4, shrinkA=2, shrinkB=6))

ax.annotate('čista vožnja: razmak se ustali na 2,9 j', xy=(20.5, g1[-1]),
            xytext=(0, 10), textcoords='offset points', ha='center',
            fontsize=9.5, color=BLUE, fontweight='bold')
ax.annotate('jedno posrtanje: razmak padne na 1,5 j,\nali čuvar ne stigne u dohvat',
            xy=(13.6, g2[int(13.6 / DT)]), xytext=(16.4, 1.85), ha='left',
            fontsize=9.5, color=ORANGE, fontweight='bold',
            arrowprops=dict(arrowstyle='->', color=ORANGE, lw=1.4, shrinkA=2, shrinkB=4))

ax.legend(loc='upper left', frameon=False, fontsize=9.5, labelcolor=MUTED,
          handlelength=1.8, ncol=3, columnspacing=1.4)
ax.set_title('Razmak čuvara i igrača kroz vrijeme (razina 1)',
             fontsize=12, color=INK, fontweight='bold', loc='left', pad=12)

ax.set_xlim(0, T_END)
ax.set_ylim(0, 6.3)
ax.set_xlabel('vrijeme (s)', fontsize=10, color=MUTED)
ax.set_ylabel('razmak čuvar – igrač (jedinice)', fontsize=10, color=MUTED)
ax.grid(axis='y', color=GRID, lw=0.9)
ax.set_axisbelow(True)
for s in ('top', 'right'):
    ax.spines[s].set_visible(False)
for s in ('left', 'bottom'):
    ax.spines[s].set_color(GRID)
ax.tick_params(colors=MUTED, labelsize=9)

fig.tight_layout()
fig.savefig(OUT + '/dijagram_potjera.png', facecolor=fig.get_facecolor())
print('dijagram_potjera.png spremljen')
