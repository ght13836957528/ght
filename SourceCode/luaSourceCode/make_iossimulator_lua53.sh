mkdir -p build_iosSimulator && cd build_iosSimulator
cmake -DCMAKE_TOOLCHAIN_FILE=../cmake/ios.toolchain.cmake -DPLATFORM=SIMULATOR64 -GXcode ../
cd ..
cmake --build build_iosSimulator --config Debug
mkdir -p plugin_lua53/Plugins/iOS/
cp build_iosSimulator/Debug-iphonesimulator/libxlua.a plugin_lua53/Plugins/iOS/libxlua.a 

