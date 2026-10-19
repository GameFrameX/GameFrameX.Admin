import {nextTick} from 'vue';
import {fas} from '@fortawesome/free-solid-svg-icons';
import {far} from '@fortawesome/free-regular-svg-icons';
import {fab} from '@fortawesome/free-brands-svg-icons';

const getFontAwesomeIconList = (prefix: string, icons: any): string[] => {
  return Object.keys(icons).map((key) => `${prefix} ${icons[key].iconName}`);
};

const getFontAwesomeSolidIcons = () => {
  return new Promise((resolve) => {
    nextTick(() => {
      resolve(getFontAwesomeIconList('fa-solid', fas));
    });
  });
};

const getFontAwesomeRegularIcons = () => {
  return new Promise((resolve) => {
    nextTick(() => {
      resolve(getFontAwesomeIconList('fa-regular', far));
    });
  });
};

const getFontAwesomeBrandsIcons = () => {
  return new Promise((resolve) => {
    nextTick(() => {
      resolve(getFontAwesomeIconList('fa-brands', fab));
    });
  });
};

const fontAwesomeIcons = {
  solid: () => getFontAwesomeSolidIcons(),
  regular: () => getFontAwesomeRegularIcons(),
  brands: () => getFontAwesomeBrandsIcons(),
};

export default fontAwesomeIcons;
