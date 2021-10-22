set(CMAKE_SYSTEM_NAME Linux)
set(CMAKE_C_COMPILER ${CROSS_COMPILE}gcc)
add_link_options(-static)
