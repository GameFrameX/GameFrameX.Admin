import zhCNData from 'primelocale/zh-CN.json';
import enData from 'primelocale/en.json';
import zhTWData from 'primelocale/zh-TW.json';
import jaData from 'primelocale/ja.json';
import koData from 'primelocale/ko.json';

type PrimeVueLocale = Record<string, unknown>;

const primeVueLocales: Record<string, PrimeVueLocale> = {
  'zh-cn': zhCNData['zh-CN'],
  en: enData.en,
  'zh-tw': zhTWData['zh-TW'],
  ja: jaData.ja,
  ko: koData.ko,
};

export function getPrimeVueLocale(lang: string): PrimeVueLocale {
  return primeVueLocales[lang] ?? primeVueLocales['zh-cn'];
}
