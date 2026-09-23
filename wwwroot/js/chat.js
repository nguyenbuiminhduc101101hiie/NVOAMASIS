// Chat nội bộ — helpers cho Components/Chat (cuộn, phím Enter, trạng thái hiển thị trang).
window.nvoChat = (function () {
    const NEAR_BOTTOM_PX = 80;
    const LOAD_OLDER_PX = 60;

    function scrollToBottom(el) {
        if (el) el.scrollTop = el.scrollHeight;
    }

    function isNearBottom(el) {
        if (!el) return true;
        return el.scrollHeight - el.scrollTop - el.clientHeight < NEAR_BOTTOM_PX;
    }

    function captureScroll(el) {
        return el ? [el.scrollHeight, el.scrollTop] : [0, 0];
    }

    // Giữ nguyên vị trí đang đọc sau khi chèn tin cũ lên đầu danh sách.
    function restoreScroll(el, prevHeight, prevTop) {
        if (el) el.scrollTop = el.scrollHeight - prevHeight + prevTop;
    }

    function scrollToMessage(id) {
        const target = document.getElementById('chat-msg-' + id);
        if (!target) return;
        target.scrollIntoView({ block: 'center' });
        target.classList.add('chat-msg--flash');
        setTimeout(() => target.classList.remove('chat-msg--flash'), 1600);
    }

    function watchScroll(el, dotnetRef) {
        if (!el || el._nvoChatScroll) return;
        let pending = false;
        el._nvoChatScroll = () => {
            if (pending || el.scrollTop > LOAD_OLDER_PX) return;
            pending = true;
            dotnetRef.invokeMethodAsync('LoadOlderFromJs').finally(() => { pending = false; });
        };
        el.addEventListener('scroll', el._nvoChatScroll, { passive: true });
    }

    // Enter gửi, Shift+Enter xuống dòng; textarea tự giãn tối đa 160px.
    function attachComposer(textarea, dotnetRef) {
        if (!textarea || textarea._nvoChatComposer) return;
        const grow = () => {
            textarea.style.height = 'auto';
            textarea.style.height = Math.min(textarea.scrollHeight, 160) + 'px';
        };
        textarea._nvoChatComposer = (e) => {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                dotnetRef.invokeMethodAsync('SubmitFromKeyboard');
            }
        };
        textarea.addEventListener('keydown', textarea._nvoChatComposer);
        textarea.addEventListener('input', grow);
    }

    function resetComposer(textarea) {
        if (!textarea) return;
        textarea.value = '';
        textarea.style.height = 'auto';
        textarea.focus();
    }

    function focus(el) {
        if (el) el.focus();
    }

    function isPageVisible() {
        return document.visibilityState === 'visible';
    }

    function watchVisibility(dotnetRef) {
        const handler = () => dotnetRef.invokeMethodAsync('OnPageVisibilityChanged', isPageVisible());
        document.addEventListener('visibilitychange', handler);
        return { dispose: () => document.removeEventListener('visibilitychange', handler) };
    }

    return {
        scrollToBottom, isNearBottom, captureScroll, restoreScroll, scrollToMessage,
        watchScroll, attachComposer, resetComposer, focus, isPageVisible, watchVisibility
    };
})();
