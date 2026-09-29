
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
window.JNLShipmentColorStorageKey = "JNLShipmentGridColors";

window.JNLApplyShipmentColorSettings = (headerBg, selectedBg, tabToolbarBg) => {
    const resolvedHeaderBg = headerBg || "#0077b6";
    const resolvedSelectedBg = selectedBg || "#1E88E5";
    const resolvedTabToolbarBg = tabToolbarBg || "#ffffff";

    document.documentElement.style.setProperty("--JNL-table-header-bg", resolvedHeaderBg);
    document.documentElement.style.setProperty("--JNL-selected-row-bg", resolvedSelectedBg);
    document.documentElement.style.setProperty("--JNL-tab-toolbar-bg", resolvedTabToolbarBg);

    try {
        const data = {
            headerBg: resolvedHeaderBg,
            selectedBg: resolvedSelectedBg,
            tabToolbarBg: resolvedTabToolbarBg
        };
        localStorage.setItem(window.JNLShipmentColorStorageKey, JSON.stringify(data));
    } catch (e) {
        // ignore localStorage errors
    }
};

window.JNLGetShipmentColorSettings = () => {
    try {
        const raw = localStorage.getItem(window.JNLShipmentColorStorageKey);
        if (!raw) return null;
        return JSON.parse(raw);
    } catch (e) {
        return null;
    }
};

window.JNLLoadShipmentColorSettings = () => {
    const settings = window.JNLGetShipmentColorSettings();
    if (settings && typeof settings === "object") {
        window.JNLApplyShipmentColorSettings(
            settings.headerBg,
            settings.selectedBg,
            settings.tabToolbarBg
        );
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
