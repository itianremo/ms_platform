window.onload = function () {
    const sidebar = document.querySelector('.hc-aside');

    // 1. Inject Custom Links (Back to Admin / Close Tab)
    const injectSidebarLinks = () => {
        const sidebar = document.querySelector('.hc-aside');
        if (!sidebar) return;

        // Check if already injected
        if (document.querySelector('.custom-sidebar-links')) return;

        const customLinksContainer = document.createElement('div');
        customLinksContainer.className = 'custom-sidebar-links';

        // Link: Go to Global Admin
        const adminLink = document.createElement('a');
        adminLink.className = 'hc-menu-item custom-link';
        adminLink.href = 'http://localhost:3000';
        adminLink.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 12px;"><rect width="7" height="9" x="3" y="3" rx="1" /><rect width="7" height="5" x="14" y="3" rx="1" /><rect width="7" height="9" x="14" y="12" rx="1" /><rect width="7" height="5" x="3" y="16" rx="1" /></svg>
            <span>Back to Admin</span>
        `;
        customLinksContainer.appendChild(adminLink);

        // Sidebar Footer (Close Tab)
        const closeLink = document.createElement('div');
        closeLink.className = 'hc-menu-item custom-link';
        closeLink.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 12px;"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
            <span>Close Tab</span>
        `;
        closeLink.style.cursor = 'pointer';
        closeLink.onclick = function () {
            window.close();
        };
        customLinksContainer.appendChild(closeLink);

        sidebar.appendChild(customLinksContainer);
    };

    // Run initially
    injectSidebarLinks();

    // 2. Enhance Column Names (Persistent via Observer)
    const renameHeaders = () => {
        const ths = document.querySelectorAll('.hc-checks-table th, .hc-checks-table-container th');
        ths.forEach(th => {
            let text = th.innerText.trim();
            let newText = null;

            // Fix specific Material Icon headers
            if (text.includes('add_circle_outline')) {
                newText = '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>';
                // Direct innerHTML replacement needed as it contains HTML
                if (th.innerHTML !== newText) th.innerHTML = newText;
                return; // Skip remaining text logic
            } else {
                // Standardize to Title Case
                text = text.toUpperCase();
                if (text.includes('SERVICE NAME') || text === 'NAME') newText = 'Service Name';
                else if (text === 'STATUS' || text === 'HEALTH') newText = 'Status';
                else if (text.includes('ON STATE FROM') || text.includes('DURATION')) newText = 'Latency';
                else if (text.includes('LAST EXECUTION')) newText = 'Last Checked';
                else if (text === 'DESCRIPTION' || text === 'DETAILS') newText = 'Diagnostics';
                else if (text === 'TAGS') newText = 'Tags';
            }

            // Only update if changed prevents infinite loop
            if (newText && th.innerHTML !== newText) {
                th.innerHTML = newText;
            }
        });
    };

    // Run initially
    setTimeout(renameHeaders, 500);
    setTimeout(renameHeaders, 2000);

    // Watch for DOM changes (Vue re-renders)
    const observer = new MutationObserver((mutations) => {
        // Debounce or check meaningful changes
        let shouldRename = false;
        mutations.forEach((mutation) => {
            // Ignore our own custom link injection to prevent loops
            if (mutation.target.closest && mutation.target.closest('.custom-sidebar-links')) return;

            if (mutation.addedNodes.length > 0 || mutation.type === 'characterData') {
                shouldRename = true;
            }
        });

        if (shouldRename) {
            renameHeaders();
            injectSidebarLinks();
            customizeIcon();
        }
    });

    const targetNode = document.querySelector('#app') || document.body;
    observer.observe(targetNode, { childList: true, subtree: true });

    // 3. Customize Collapse Icons
    const customizeIcon = () => {
        // Main Toggle Button
        const toggler = document.querySelector('.hc-menu-toggler');
        if (toggler) {
            const text = toggler.textContent.trim();

            // Define SVGs
            const closeIconSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2" ry="2"/><path d="M9 3v18"/><path d="m16 15-3-3 3-3"/></svg>`; // Arrow pointing left
            const openIconSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2" ry="2"/><path d="M9 3v18"/><path d="m14 15 3-3-3-3"/></svg>`; // Arrow pointing right

            // If the text is exactly one of the known material ligatures, replace it.
            // We check innerHTML to avoid replacing our own SVG repeatedly.
            if (!toggler.innerHTML.includes('<svg')) {
                if (text.includes('menu_open')) {
                    toggler.innerHTML = closeIconSvg;
                } else if (text.includes('menu') || text.includes('menu_close')) {
                    toggler.innerHTML = openIconSvg;
                } else {
                    // Default fallback
                    toggler.innerHTML = closeIconSvg;
                }
                toggler.classList.remove('material-icons');
            }
        }
    };

    // Run periodically
    setTimeout(customizeIcon, 500);
    setInterval(customizeIcon, 2000);
};
