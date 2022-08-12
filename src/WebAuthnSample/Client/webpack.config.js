const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const path = require('path');

module.exports = function(env, argv) {
    const mode = argv.mode || "development";
    
    let config = {
        mode: mode === "production" ? "production": "development",
        devtool: mode === "production" ? "source-map": "eval-cheap-module-source-map",
        plugins: [
            new MiniCssExtractPlugin({
                filename: "css/shared.css"
            })
        ],
        entry: {
            shared: {
                import: ["react", "react-dom", "./shared/index.ts"]
            },
            registration: {
                import: "./registration/index.tsx",
                dependOn: ["shared"]
            }
        },
        output: {
            path: path.resolve(__dirname, "../wwwroot"),
            filename: "js/[name].js"
        },
        resolve: {
            extensions: [ ".ts", ".tsx", ".js", ".jsx", ".json"]
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    loader: "babel-loader"
                },
                {
                    test: /\.scss$/,
                    use: [
                        { loader: MiniCssExtractPlugin.loader },
                        "css-loader"
                    ]
                }
            ]
        }
    };

    return config;
};