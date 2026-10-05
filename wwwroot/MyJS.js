
window.openReportInNewTab = function (pdfData) {
    const byteArray = new Uint8Array(pdfData);
    const blob = new Blob([byteArray], { type: "application/pdf" });
    const url = URL.createObjectURL(blob);
    window.open(url, "_blank");
}

window.saveAsFile = (fileName, byteBase64) => {
    const byteCharacters = atob(byteBase64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
}


window.setFontSize = (fontSize) => {
    document.documentElement.style.fontSize = fontSize;
}

window.triggerFileInputClick = (element) => {
    element.click();
}

window.triggerFileUpload = (element) => {
    if (element) {
        element.click();
    }
}
window.disableScroll = function () {
    document.body.style.overflow = "hidden";
}

window.enableScroll = function () {
    document.body.style.overflow = "";
}

window.updateFontSize = (headerh6,noidung,tieude,noidungluoi) => {
    document.documentElement.style.setProperty('--mud-typography-h6-size', headerh6);
    document.documentElement.style.setProperty('--mud-input-root-size', noidung);
    document.documentElement.style.setProperty('--mud-input-label-size', tieude);
    document.documentElement.style.setProperty('--mud-input-table-cell-size', noidungluoi);
}

window.updateNavMenuFontSize = (navmenu) => {
    document.documentElement.style.setProperty('--nav-menu-font-size', navmenu);
}

window.setPageTitle = (title) => {
    document.title = title;
}

window.downloadFileFromStream = (filename, base64Data) => {
    const link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.openUrlInNewTab = (url) => {
    if (url) window.open(url, "_blank", "noopener,noreferrer");
}

window.getPublicIp = async function () {
    try {
        let response = await fetch("https://api.ipify.org?format=json");
        let data = await response.json();
        return data.ip;
    } catch (e) {
        console.error("Không lấy được IP:", e);
        return "unknown";
    }
}

window.printInventoryHtml = function (htmlContent) {
    const w = window.open("", "_blank");
    if (!w) return;
    w.document.write(htmlContent);
    w.document.close();
    w.focus();
    setTimeout(function () { w.print(); }, 300);
};

window.downloadFileFromUrl = function (url) {
    const fileName = url.split('/').pop(); // lấy tên file từ URL

    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;      // gợi ý tên file khi tải về
    a.target = "_blank";        // mở trong tab mới nếu cần
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};


// ========== AI ASSISTANT FUNCTIONS ==========
window.scrollChatToBottom = function () {
    var el = document.getElementById('aiChatMessages');
    if (el) {
        el.scrollTop = el.scrollHeight;
    }
};

window.copyToClipboard = function (text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        return navigator.clipboard.writeText(text);
    }
    // Fallback
    var textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
};

// Shipment grid/tab colors: apply globally via CSS variables.
// Stored in localStorage so the color persists after refresh.
// Lưu ý: commit b5e630f đổi tên hàm sang JNL* nhưng C# (MainLayout, ShipmentGridColorSelector) và app.css
// vẫn dùng tên nvocc* / --nvocc-* → trình duyệt báo "Could not find 'nvoccLoadShipmentColorSettings'".
// Giữ MỘT bản cài đặt và xuất ra cả hai bộ tên; đặt cả hai bộ biến CSS.
(function () {
    // Khóa đang dùng từ sau b5e630f; khóa cũ chỉ đọc khi chưa có khóa mới.
    const STORAGE_KEY = "JNLShipmentGridColors";
    const LEGACY_KEYS = ["nvoccShipmentGridColors"];

    const apply = (headerBg, selectedBg, tabToolbarBg) => {
        const resolvedHeaderBg = headerBg || "#0077b6";
        const resolvedSelectedBg = selectedBg || "#1E88E5";
        const resolvedTabToolbarBg = tabToolbarBg || "#ffffff";

        const root = document.documentElement.style;
        for (const prefix of ["--nvocc", "--JNL"]) {
            root.setProperty(prefix + "-table-header-bg", resolvedHeaderBg);
            root.setProperty(prefix + "-selected-row-bg", resolvedSelectedBg);
            root.setProperty(prefix + "-tab-toolbar-bg", resolvedTabToolbarBg);
        }

        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify({
                headerBg: resolvedHeaderBg,
                selectedBg: resolvedSelectedBg,
                tabToolbarBg: resolvedTabToolbarBg
            }));
        } catch (e) {
            // ignore localStorage errors
        }
    };

    const get = () => {
        try {
            for (const key of [STORAGE_KEY, ...LEGACY_KEYS]) {
                const raw = localStorage.getItem(key);
                if (raw) return JSON.parse(raw);
            }
        } catch (e) {
            // ignore
        }
        return null;
    };

    const load = () => {
        const settings = get();
        if (settings && typeof settings === "object") {
            apply(settings.headerBg, settings.selectedBg, settings.tabToolbarBg);
        }
    };

    window.nvoccShipmentColorStorageKey = STORAGE_KEY;
    window.JNLShipmentColorStorageKey = STORAGE_KEY;
    window.nvoccApplyShipmentColorSettings = apply;
    window.JNLApplyShipmentColorSettings = apply;
    window.nvoccGetShipmentColorSettings = get;
    window.JNLGetShipmentColorSettings = get;
    window.nvoccLoadShipmentColorSettings = load;
    window.JNLLoadShipmentColorSettings = load;
})();

// Kéo giãn menu bên trái (MainLayout).
// Toàn bộ thao tác kéo chạy ở trình duyệt: mỗi khung hình chỉ đổi CSS variable độ rộng của MudDrawer / MudLayout,
// không gửi từng chuyển động chuột về server (trước đây mỗi lần nhích chuột = 1 vòng SignalR + vẽ lại cả layout → giật).
// Thả chuột mới lưu localStorage và báo .NET đúng 1 lần (OnDrawerResized).
window.nvoccDrawerResize = {
    attach(handle, dotnetRef, options) {
        if (!handle || handle.__nvoccDrawerResize) return;
        const opt = Object.assign({ min: 180, max: 480, defaultWidth: 240, storageKey: "nvocc.drawerWidth" }, options || {});
        const doc = document.documentElement;
        const findDrawer = () => document.querySelector(".mud-drawer.mud-drawer-pos-left") || document.querySelector(".mud-drawer");
        const findLayout = () => handle.closest(".mud-layout") || document.querySelector(".mud-layout");
        const clamp = (w) => Math.round(Math.min(opt.max, Math.max(opt.min, w)));

        let pointerId = null, startX = 0, startW = 0, current = 0, frame = 0;

        // Độ rộng đích (biến CSS), không đọc kích thước thật vì có thể đang chạy hiệu ứng co giãn của MudBlazor
        const currentWidth = () => {
            const layout = findLayout();
            const v = layout ? parseFloat(getComputedStyle(layout).getPropertyValue("--mud-drawer-width-left")) : NaN;
            if (!isNaN(v) && v > 0) return v;
            const drawer = findDrawer();
            return drawer ? drawer.getBoundingClientRect().width : opt.defaultWidth;
        };

        const apply = (w) => {
            const drawer = findDrawer();
            const layout = findLayout();
            if (drawer) drawer.style.setProperty("--mud-drawer-width", w + "px");
            if (layout) layout.style.setProperty("--mud-drawer-width-left", w + "px");
        };

        const commit = (w) => {
            try { localStorage.setItem(opt.storageKey, String(w)); } catch (e) { /* ignore */ }
            if (dotnetRef) dotnetRef.invokeMethodAsync("OnDrawerResized", w).catch(() => { });
            // để các lưới / biểu đồ tự tính lại kích thước theo độ rộng mới
            window.dispatchEvent(new Event("resize"));
        };

        const onDown = (e) => {
            if (e.button !== 0) return;
            if (!findDrawer()) return;
            pointerId = e.pointerId;
            startX = e.clientX;
            startW = clamp(currentWidth());
            current = startW;
            try { handle.setPointerCapture(pointerId); } catch (err) { /* ignore */ }
            doc.classList.add("nvocc-drawer-resizing");
            e.preventDefault();
        };

        const onMove = (e) => {
            if (pointerId === null || e.pointerId !== pointerId) return;
            current = clamp(startW + (e.clientX - startX));
            if (!frame) frame = requestAnimationFrame(() => { frame = 0; apply(current); });
            e.preventDefault();
        };

        const onUp = () => {
            if (pointerId === null) return;
            try { handle.releasePointerCapture(pointerId); } catch (err) { /* ignore */ }
            pointerId = null;
            if (frame) { cancelAnimationFrame(frame); frame = 0; }
            apply(current);
            doc.classList.remove("nvocc-drawer-resizing");
            if (current !== startW) commit(current); // bấm không kéo (vd trong nhấp đúp) thì không báo server
        };

        const onDblClick = () => {
            current = clamp(opt.defaultWidth);
            apply(current);
            commit(current);
        };

        const onKey = (e) => {
            if (e.key !== "ArrowLeft" && e.key !== "ArrowRight") return;
            if (!findDrawer()) return;
            const step = e.shiftKey ? 48 : 16;
            current = clamp(currentWidth() + (e.key === "ArrowRight" ? step : -step));
            apply(current);
            commit(current);
            e.preventDefault();
        };

        handle.addEventListener("pointerdown", onDown);
        handle.addEventListener("pointermove", onMove);
        handle.addEventListener("pointerup", onUp);
        handle.addEventListener("pointercancel", onUp);
        handle.addEventListener("lostpointercapture", onUp);
        handle.addEventListener("dblclick", onDblClick);
        handle.addEventListener("keydown", onKey);

        handle.__nvoccDrawerResize = {
            detach() {
                handle.removeEventListener("pointerdown", onDown);
                handle.removeEventListener("pointermove", onMove);
                handle.removeEventListener("pointerup", onUp);
                handle.removeEventListener("pointercancel", onUp);
                handle.removeEventListener("lostpointercapture", onUp);
                handle.removeEventListener("dblclick", onDblClick);
                handle.removeEventListener("keydown", onKey);
                doc.classList.remove("nvocc-drawer-resizing");
                delete handle.__nvoccDrawerResize;
            }
        };
    },

    detach(handle) {
        if (handle && handle.__nvoccDrawerResize) handle.__nvoccDrawerResize.detach();
    }
};

// BKAV invoice credentials: browser-only PartnerGuid/PartnerToken (localStorage).
window.nvoccBkavInvoiceCredentialsStorageKey = "nvocc.bkavInvoiceCredentials";

window.nvoccGetBkavInvoiceCredentials = () => {
    try {
        const raw = localStorage.getItem(window.nvoccBkavInvoiceCredentialsStorageKey);
        if (!raw) return null;
        const data = JSON.parse(raw);
        if (!data || typeof data !== "object") return null;
        return {
            partnerGuid: data.partnerGuid || "",
            partnerToken: data.partnerToken || "",
            savedAt: data.savedAt || null
        };
    } catch (e) {
        return null;
    }
};

window.nvoccSetBkavInvoiceCredentials = (partnerGuid, partnerToken) => {
    const data = {
        partnerGuid: (partnerGuid || "").trim(),
        partnerToken: (partnerToken || "").trim(),
        savedAt: new Date().toISOString()
    };
    try {
        localStorage.setItem(window.nvoccBkavInvoiceCredentialsStorageKey, JSON.stringify(data));
    } catch (e) {
        // ignore localStorage errors
    }
    return data;
};

window.nvoccClearBkavInvoiceCredentials = () => {
    try {
        localStorage.removeItem(window.nvoccBkavInvoiceCredentialsStorageKey);
    } catch (e) {
        // ignore localStorage errors
    }
};

// View Document: scroll tới row được highlight bằng class "shipment-selected-row" hoặc "vd-row-target"
// Hỗ trợ MudDataGrid có Virtualize="true": scroll container tới đúng vị trí.
window.JNLScrollToShipmentRow = function (jobId) {
    setTimeout(function () {
        try {
            // Tìm row có class shipment-selected-row (được set khi user click hoặc khi mình set jobdetail)
            var row =
                document.querySelector('tr.shipment-selected-row') ||
                document.querySelector('tr.selected') ||
                document.querySelector('tr.mud-selected');

            if (!row) return;

            // Thêm class flash để gây chú ý
            row.classList.add('vd-row-flash');
            setTimeout(function () { row.classList.remove('vd-row-flash'); }, 1800);

            // scrollIntoView với behavior smooth, block center
            row.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'nearest' });
        } catch (e) {
            // ignore
        }
    }, 50);
};

// Tên tab trình duyệt luôn bắt đầu bằng tên phần mềm: "AMASIS - NVOCC | <tên màn hình>".
// Mỗi màn hình tự đặt <PageTitle> (vd "Shipment"); đoạn này tự thêm tên phần mềm phía trước
// mỗi khi tiêu đề đổi, nên không phải sửa từng màn hình.
// - Tự chạy khi tải trang (không phụ thuộc MainLayout); tên lấy từ <meta name="app-brand"> trong App.razor.
// - Sửa trực tiếp nút chữ bên trong <title> (không gán document.title) để Blazor vẫn cập nhật được tiêu đề.
window.__nvoccApplyBrandTitle = () => {
    const brand = window.__nvoccBrand;
    if (!brand) return;
    const sep = ' | ';
    let el = document.head.querySelector('title');
    if (!el) {
        el = document.createElement('title');
        document.head.appendChild(el);
    }
    let t = (el.textContent || '').trim();
    if (t === brand || t.startsWith(brand + sep)) return;
    // Bỏ các tên phần mềm cũ ở đầu (khi đổi tên lúc đang mở trang) để không bị "Mới | Cũ | Shipment"
    for (const old of (window.__nvoccOldBrands || [])) {
        if (t === old) t = '';
        else if (t.startsWith(old + sep)) t = t.substring(old.length + sep.length);
    }
    const text = t ? brand + sep + t : brand;
    const node = el.firstChild;
    if (node && node.nodeType === Node.TEXT_NODE && el.childNodes.length === 1) node.nodeValue = text;
    else el.textContent = text;
};

window.nvoccSetBrandTitle = (brand) => {
    brand = (brand || '').trim();
    if (!brand) return;
    const prev = window.__nvoccBrand;
    if (prev && prev !== brand) (window.__nvoccOldBrands = window.__nvoccOldBrands || []).push(prev);
    window.__nvoccBrand = brand;
    window.__nvoccApplyBrandTitle();
    if (!window.__nvoccTitleObserver) {
        window.__nvoccTitleObserver = new MutationObserver(() => window.__nvoccApplyBrandTitle());
        window.__nvoccTitleObserver.observe(document.head, { subtree: true, childList: true, characterData: true });
    }
};

(function () {
    const start = () => {
        const meta = document.querySelector('meta[name="app-brand"]');
        window.nvoccSetBrandTitle((meta && meta.content) || 'AMASIS - NVOCC');
    };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
    else start();
})();
