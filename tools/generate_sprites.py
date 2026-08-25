"""
Generira jednostavne placeholder sprite-ove za "Run, Slavko, Run!".
Ovo NIJE finalna umjetnost — samo prepoznatljivi geometrijski oblici u boji
da se mehanike mogu testirati u Unity Editoru prije nego što se doda pravi 2D art.

Pokreni: python3 generate_sprites.py  (potreban Pillow: pip install pillow)
"""

from PIL import Image, ImageDraw
import os

ROOT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Sprites")


def save(img, *path_parts):
    path = os.path.join(ROOT, *path_parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    print("Saved:", path)


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


# ---------------------------------------------------------------------------
# SLAVKO (majmun) — jednostavna stojeća pozicija, gleda desno
# ---------------------------------------------------------------------------
def make_slavko():
    w, h = 128, 160
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)

    body_color = (139, 90, 43, 255)      # smeđa
    belly_color = (222, 184, 135, 255)   # svjetlija smeđa (trbuh)
    outline = (60, 35, 15, 255)

    # tijelo
    d.ellipse([28, 55, 100, 150], fill=body_color, outline=outline, width=3)
    # trbuh
    d.ellipse([44, 80, 84, 140], fill=belly_color)
    # glava
    d.ellipse([30, 10, 98, 78], fill=body_color, outline=outline, width=3)
    # lice
    d.ellipse([42, 30, 86, 68], fill=belly_color)
    # uši
    d.ellipse([18, 18, 42, 42], fill=body_color, outline=outline, width=2)
    d.ellipse([86, 18, 110, 42], fill=body_color, outline=outline, width=2)
    # oči
    d.ellipse([50, 40, 60, 50], fill=(20, 20, 20, 255))
    d.ellipse([70, 40, 80, 50], fill=(20, 20, 20, 255))
    # ruke
    d.ellipse([10, 70, 34, 120], fill=body_color, outline=outline, width=3)
    d.ellipse([94, 70, 118, 120], fill=body_color, outline=outline, width=3)
    # noge
    d.ellipse([34, 140, 56, 160], fill=body_color, outline=outline, width=2)
    d.ellipse([72, 140, 94, 160], fill=body_color, outline=outline, width=2)

    save(img, "Slavko", "slavko_idle.png")


# ---------------------------------------------------------------------------
# CUVAR (guard) — humanoidni silhouette, hladna paleta boja
# ---------------------------------------------------------------------------
def make_guard():
    w, h = 128, 160
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)

    uniform = (46, 62, 90, 255)     # tamno plava uniforma
    skin = (222, 184, 160, 255)
    outline = (20, 25, 40, 255)
    cap = (30, 40, 60, 255)

    # noge
    d.rectangle([44, 120, 60, 158], fill=uniform, outline=outline, width=2)
    d.rectangle([68, 120, 84, 158], fill=uniform, outline=outline, width=2)
    # tijelo
    d.rectangle([36, 62, 92, 128], fill=uniform, outline=outline, width=3)
    # ruke
    d.rectangle([16, 65, 36, 115], fill=uniform, outline=outline, width=2)
    d.rectangle([92, 65, 112, 115], fill=uniform, outline=outline, width=2)
    # glava
    d.ellipse([44, 20, 84, 62], fill=skin, outline=outline, width=3)
    # kapa
    d.rectangle([40, 12, 88, 28], fill=cap, outline=outline, width=2)
    d.ellipse([40, 6, 88, 24], fill=cap, outline=outline, width=2)
    # oči (ljuti pogled)
    d.line([52, 40, 60, 36], fill=(20, 20, 20, 255), width=3)
    d.line([68, 36, 76, 40], fill=(20, 20, 20, 255), width=3)

    save(img, "Guards", "guard_idle.png")


# ---------------------------------------------------------------------------
# POWER-UPOVI
# ---------------------------------------------------------------------------
def make_banana():
    w, h = 64, 64
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.arc([6, 6, 58, 70], start=200, end=340, fill=(240, 210, 40, 255), width=14)
    d.ellipse([4, 4, 60, 60], outline=(180, 140, 20, 255), width=0)
    save(img, "PowerUps", "banana_boost.png")


def make_kokos():
    w, h = 64, 64
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.ellipse([6, 6, 58, 58], fill=(101, 67, 33, 255), outline=(60, 40, 20, 255), width=3)
    d.ellipse([20, 20, 44, 44], fill=(245, 245, 235, 255))
    d.ellipse([28, 28, 36, 36], fill=(60, 40, 20, 255))
    save(img, "PowerUps", "kokos.png")


def make_powerape():
    w, h = 64, 64
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.ellipse([4, 4, 60, 60], fill=(200, 60, 40, 255), outline=(120, 25, 15, 255), width=3)
    # simboličan "fist" - krug s crticama za prste
    d.ellipse([16, 20, 48, 50], fill=(230, 190, 160, 255))
    for x in [18, 26, 34, 42]:
        d.rectangle([x, 10, x + 6, 24], fill=(230, 190, 160, 255))
    save(img, "PowerUps", "power_ape.png")


# ---------------------------------------------------------------------------
# PREPREKE (jedna generička nelomljiva + jedna lomljiva, po zoni)
# ---------------------------------------------------------------------------
def make_obstacle(name, color, outline):
    w, h = 96, 96
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.rectangle([8, 24, 88, 88], fill=color, outline=outline, width=4)
    d.line([8, 24, 88, 88], fill=outline, width=2)
    d.line([88, 24, 8, 88], fill=outline, width=2)
    save(img, "Obstacles", f"{name}.png")


def make_breakable(name, color, outline):
    w, h = 96, 96
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.rectangle([8, 24, 88, 88], fill=color, outline=outline, width=4)
    # "napuklina" da se vidi da je lomljivo
    d.line([48, 24, 30, 50, ], fill=outline, width=3)
    d.line([30, 50, 60, 65], fill=outline, width=3)
    d.line([60, 65, 40, 88], fill=outline, width=3)
    save(img, "Obstacles", f"{name}.png")


# ---------------------------------------------------------------------------
# HANG POINT marker (grana / uže / bandera - generički krug-marker za editor)
# ---------------------------------------------------------------------------
def make_hangpoint():
    w, h = 32, 32
    img = new_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.ellipse([4, 4, 28, 28], fill=(255, 210, 0, 200), outline=(120, 90, 0, 255), width=2)
    save(img, "..", "HangPointMarker.png")


# ---------------------------------------------------------------------------
# POZADINE (jednostavan gradient + tlo, po tematici razine) — 1024x576
# ---------------------------------------------------------------------------
def make_background(level_folder, sky_top, sky_bottom, ground_color, filename="bg_layer_far.png"):
    w, h = 1024, 576
    img = Image.new("RGBA", (w, h), (0, 0, 0, 255))
    d = ImageDraw.Draw(img)

    ground_h = 120
    sky_h = h - ground_h
    for y in range(sky_h):
        t = y / max(sky_h - 1, 1)
        r = int(sky_top[0] + (sky_bottom[0] - sky_top[0]) * t)
        g = int(sky_top[1] + (sky_bottom[1] - sky_top[1]) * t)
        b = int(sky_top[2] + (sky_bottom[2] - sky_top[2]) * t)
        d.line([(0, y), (w, y)], fill=(r, g, b, 255))

    d.rectangle([0, sky_h, w, h], fill=ground_color)

    save(img, "Backgrounds", level_folder, filename)


def main():
    make_slavko()
    make_guard()

    make_banana()
    make_kokos()
    make_powerape()

    # Zoo (razina 1)
    make_obstacle("zoo_crate", (150, 110, 60, 255), (90, 60, 25, 255))
    make_breakable("zoo_wooden_fence_breakable", (170, 130, 80, 255), (90, 60, 25, 255))
    make_background("Zoo", (120, 170, 220), (200, 220, 180), (90, 140, 70, 255))

    # City (razina 2)
    make_obstacle("city_traffic_cone", (230, 110, 20, 255), (120, 50, 5, 255))
    make_breakable("city_wooden_crate_breakable", (150, 90, 40, 255), (80, 45, 15, 255))
    make_background("City", (35, 35, 60), (90, 90, 120), (55, 55, 65, 255))

    # Forest (razina 3)
    make_obstacle("forest_rock", (110, 110, 110, 255), (60, 60, 60, 255))
    make_breakable("forest_dead_branch_breakable", (95, 70, 40, 255), (55, 40, 20, 255))
    make_background("Forest", (20, 45, 25), (60, 90, 45, 255), (35, 55, 25, 255))

    # Factory (razina 4)
    make_obstacle("factory_barrel", (170, 40, 30, 255), (90, 15, 10, 255))
    make_breakable("factory_crate_breakable", (120, 100, 40, 255), (70, 55, 20, 255))
    make_background("Factory", (25, 15, 20), (70, 30, 25, 255), (30, 25, 25, 255))

    make_hangpoint()


if __name__ == "__main__":
    main()
