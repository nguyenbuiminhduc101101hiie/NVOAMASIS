# -*- coding: utf-8 -*-
"""
Quét code tìm các key ngôn ngữ (Localizer["Key"]) và màn hình đang dùng key đó.
Kết quả: wwwroot/data/localization-usage.json, được cửa sổ "Edit Language" đọc để hiện cột "Vị trí".

Chạy lại sau khi thêm / sửa màn hình:  python Scripts/gen_localization_usage.py   (đứng ở thư mục gốc project)
"""
import json, os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

def read(p):
    with open(p, 'rb') as f:
        return f.read().decode('utf-8-sig', errors='replace')

nav = read('Components/Layout/NavMenu.razor')

# Render*() helper -> component type
render_types = {}
for m in re.finditer(r'RenderFragment\s+(Render\w+)\(\)\s*=>\s*builder\s*=>\s*\{(.*?)\};', nav, re.S):
    t = re.search(r'OpenComponent<([\w.]+)>|OpenComponent\(\s*\d+\s*,\s*typeof\(([\w.]+)\)\)', m.group(2))
    if t:
        render_types[m.group(1)] = t.group(1) or t.group(2)

# Menu entries: OpenTab("key", $"6.1 {Localizer["Label"]}", RenderComponent(typeof(X)) | RenderX())
menu = []  # (num, labelKey, labelText, typeName)
pat = re.compile(r'OpenTab\(\s*"[^"]*"\s*,\s*\$?"(?P<num>[\d.]+)\s*(?:\{Localizer\["(?P<lk>[^"]+)"\]\}|(?P<lt>[^"{]*))\s*"\s*,\s*'
                 r'(?:RenderComponent\(typeof\((?P<type>[\w.]+)\)\)|(?P<render>Render\w+)\(\))')
for m in pat.finditer(nav):
    t = m.group('type') or render_types.get(m.group('render') or '')
    if not t:
        continue
    menu.append((m.group('num'), m.group('lk') or '', (m.group('lt') or '').strip(), t))

def type_to_path(t):
    parts = t.split('.')
    if parts[0] == 'NVOAMASIS':
        parts = parts[1:]
    return '/'.join(parts) + '.razor'

screens_by_file = {}
screens_by_folder = {}
for num, lk, lt, t in menu:
    path = type_to_path(t)
    entry = [num, lk, lt]
    screens_by_file.setdefault(path, [])
    if entry not in screens_by_file[path]:
        screens_by_file[path].append(entry)
    folder = '/'.join(path.split('/')[:2])  # Components/X
    screens_by_folder.setdefault(folder, [])
    if entry not in screens_by_folder[folder]:
        screens_by_folder[folder].append(entry)

key_pat = re.compile(r'\b(?:Localizer|_localizer|localizer|L|_L|Loc)\s*\[\s*"([^"\\]+)"\s*\]')
t_pat = re.compile(r'\bT\(\s*"([A-Za-z0-9_.]+)"\s*,')

keys = {}
files = {}
for dirpath, dirnames, filenames in os.walk('.'):
    dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj', '.git', '.vs', 'node_modules', 'wwwroot', 'Migrations')]
    for fn in filenames:
        if not fn.endswith(('.razor', '.cs')):
            continue
        p = os.path.join(dirpath, fn)[2:].replace('\\', '/')
        try:
            s = read(p)
        except Exception:
            continue
        found = set(key_pat.findall(s)) | set(t_pat.findall(s))
        if not found:
            continue
        if p in screens_by_file:
            scr = screens_by_file[p]
        else:
            folder = '/'.join(p.split('/')[:2])
            scr = screens_by_folder.get(folder, [])
        if p.startswith('Components/Layout/'):
            scr = [["", "", "Menu / thanh trên cùng"]]
        files[p] = scr
        for k in found:
            keys.setdefault(k, []).append(p)

for k in keys:
    keys[k].sort()

os.makedirs('wwwroot/data', exist_ok=True)
with open('wwwroot/data/localization-usage.json', 'w', encoding='utf-8') as f:
    json.dump({'files': files, 'keys': keys}, f, ensure_ascii=False, separators=(',', ':'), sort_keys=True)
print(f'{len(keys)} keys, {len(files)} files, {len(menu)} menu entries -> wwwroot/data/localization-usage.json')
