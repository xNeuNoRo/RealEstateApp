import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './Views/Shared/Components/**/*.cshtml',
  ],
};

export default config;
