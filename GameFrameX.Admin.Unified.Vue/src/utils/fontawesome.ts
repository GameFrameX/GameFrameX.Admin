// Webfont CSS — needed for dynamic class-based rendering (database icons, SvgIcon)
import '@fortawesome/fontawesome-free/css/all.min.css';

// SVG component library setup — for code-level icon usage with tree-shaking
import {library} from '@fortawesome/fontawesome-svg-core';
import {fas} from '@fortawesome/free-solid-svg-icons';
import {far} from '@fortawesome/free-regular-svg-icons';
import {fab} from '@fortawesome/free-brands-svg-icons';

library.add(fas, far, fab);
