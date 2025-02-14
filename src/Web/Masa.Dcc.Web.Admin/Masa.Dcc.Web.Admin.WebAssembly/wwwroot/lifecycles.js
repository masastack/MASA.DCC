/// <reference path="../../masa.tsc.web.admin.rcl/wwwroot/js/china.js" />
import singleSpaBlazor from 'blazor-wasm-single-spa';

// Build the asset base URL from this JavaScript module's URL. The asset base URL must have a
// trailing slash for Blazor to apply it correctly.
const iLastSlash = import.meta.url.lastIndexOf('/');
const assetBaseUrl = import.meta.url.substring(0, iLastSlash + 1);

export const { bootstrap, mount, unmount } = singleSpaBlazor({
    appTagName: 'mfe-tsc-app',
    stylePaths: [
        '/css/app.css',
        '_content/Masa.Tsc.Web.Admin.Rcl/css/tsc.css',
        //'https://cdn.masastack.com/npm/fontawesome/v6.4.0/css/all.css',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quill.snow.css',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quill.bubble.css',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quill-emoji.css',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quilljs-markdown-common-style.css',
        //'https://cdn.bootcdn.net/ajax/libs/gridstack.js/7.2.1/gridstack.min.css',
        //'https://cdn.bootcdn.net/ajax/libs/gridstack.js/7.2.1/gridstack-extra.min.css',
        'Masa.Tsc.Web.Admin.Wasm.styles.css'
    ],
    additionalImportPaths: [
        '_content/Masa.Blazor/js/masa-blazor.js',
        '_content/Masa.Stack.Components/js/components.js',
        '_content/Microsoft.AspNetCore.Components.WebAssembly.Authentication/AuthenticationService.js',
        //'https://gw.alipayobjects.com/os/lib/antv/g6/4.3.11/dist/g6.min.js',
        //'https://gw.alipayobjects.com/os/antv/pkg/_antv.g6-3.1.1/build/minimap.js',
        '_content/Masa.Tsc.Web.Admin.Rcl/js/autoResize.js',
        '_content/Masa.Tsc.Web.Admin.Rcl/js/gridstack/gridstack-all.js',
        '_content/Masa.Tsc.Web.Admin.Rcl/js/configuration-save-reminder.js',
        '_content/Masa.Tsc.Web.Admin.Rcl/js/upload/meditor-upload-image.js',
        '_content/Masa.Tsc.Web.Admin.Rcl/js/china.js',
        'https://cdn.masastack.com/npm/echarts/5.4.2/echarts.min.js',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quill.js',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quilljs-markdown.js',
        //'https://cdn.masastack.com/npm/quill/1.3.6/quill-emoji.js',
        //'https://cdn.masastack.com/npm/sortable/Sortable.min.js',
        //'https://gw.alipayobjects.com/os/antv/pkg/_antv.g2-3.5.5/dist/g2.min.js',
        //'https://gw.alipayobjects.com/os/antv/pkg/_antv.data-set-0.10.1/dist/data-set.min.js',
        //'https://cdn.masastack.com/npm/masonry/masonry.pkgd.min.js',
        //'https://cdn.masastack.com/npm/monaco-editor/0.34.1/min/vs/loader.js',
        //'https://cdn.masastack.com/npm/monaco-editor/0.34.1/min/vs/editor/editor.main.nls.js',
        //'https://cdn.masastack.com/npm/monaco-editor/0.34.1/min/vs/editor/editor.main.js',
    ],
    assetBaseUrl,
});
