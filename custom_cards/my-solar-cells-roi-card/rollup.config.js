import resolve from "@rollup/plugin-node-resolve";
import typescript from "@rollup/plugin-typescript";
import { terser } from "rollup-plugin-terser";

export default {
  input: "src/my-solar-cells-roi-card.ts",
  output: {
    file: "my-solar-cells-roi-card.js",
    format: "es",
  },
  plugins: [
    resolve(),
    typescript(),
    terser(),
  ],
};
