# SPDX-License-Identifier: LGPL-3.0-or-later

cmake_minimum_required(VERSION 3.13)

if(PICO_SDK_PATH)
	include(${PICO_SDK_PATH}/pico_sdk_init.cmake)
	set(CMAKE_SYSTEM_NAME PICO)
endif()

project(redforth
	VERSION 0.1.0
)

add_compile_options(-Wall)
if(${CMAKE_SYSTEM_NAME} STREQUAL Linux)
	add_compile_options(-Werror)
endif()
add_compile_options(-g)
add_compile_options(-O2)

set(FORTH_WORDS
	core-words.h
	debug-words.h
	tools-words.h
)
file(GLOB FORTH_SOURCES words/*.fs)

if(NOT CMAKE_CROSSCOMPILING)
	add_executable(crossforth
		libred/libred.c
		libred/libred-stdc.c
		libred/vm-gnuc.c
		ports/unix/main.c
	)
	target_compile_definitions(crossforth PUBLIC
		HAVE_STDC_WORDS
	)
	target_include_directories(crossforth PUBLIC
		libred/
		words/
	)

	set(CROSSFORTH $<TARGET_FILE:crossforth>)
else()
	set(CROSSFORTH ${REDFORTH_TOOLS_DIR}/crossforth)
endif()


add_custom_command(
	OUTPUT ${FORTH_WORDS}
	COMMAND ln -fs ${CMAKE_SOURCE_DIR}/words/*.fs .
	COMMAND ${CROSSFORTH}
			< ${CMAKE_SOURCE_DIR}/scripts/codegen.fs
	                > /dev/null
	DEPENDS ${FORTH_SOURCES}
)
add_custom_target(codegen
	DEPENDS ${FORTH_WORDS}
)
if(NOT CMAKE_CROSSCOMPILING)
	add_dependencies(codegen crossforth)
endif()

if(${CMAKE_SYSTEM_NAME} STREQUAL Linux)
	add_executable(redforth
		libred/libred.c
		libred/libred-linenoise.c
		libred/linenoise.c
		libred/vm-gnuc.c
		ports/unix/main.c
	)

	target_compile_definitions(redforth PUBLIC
		HAVE_STDC_WORDS
		HAVE_CODEGEN_WORDS
		HAVE_ISATTY
		HAVE_TERMIOS
		USE_READ_WRITE
	)
	target_include_directories(redforth PUBLIC
		${CMAKE_BINARY_DIR}
		libred/
		words/
	)
	add_dependencies(redforth codegen)

	include(CTest)
	add_test(
		NAME crosstest
		COMMAND sh -c "cat core-words.fs debug-words.fs tools-words.fs \
			${CMAKE_SOURCE_DIR}/scripts/selftest.fs \
			| ${CROSSFORTH}"

	)
	add_test(
		NAME selftest
		COMMAND sh -c "$<TARGET_FILE:redforth> \
			< ${CMAKE_SOURCE_DIR}/scripts/selftest.fs"
	)
	add_test(
		NAME eyeball
		COMMAND sh -c "$<TARGET_FILE:redforth> \
			< ${CMAKE_SOURCE_DIR}/scripts/eyeball.fs \
			| uniq -u > eyeball.log \
		&& \
			[ `cat eyeball.log | wc -l` -eq 1 ] \
		|| \
			(cat eyeball.log; false)"
	)
endif()

if(${CMAKE_SYSTEM_NAME} STREQUAL PICO)
	pico_sdk_init()

	add_executable(redforth
		libred/libred.c
		libred/libred-rp2.c
		libred/linenoise.c
		libred/vm-gnuc.c
		ports/rp2/main.c
	)

	target_compile_definitions(redforth PUBLIC
		HAVE_STDC_WORDS
		HAVE_RP2_WORDS
		HAVE_CODEGEN_WORDS
		PICO_ENTER_USB_BOOT_ON_EXIT
	)
	target_compile_definitions(redforth PUBLIC ENABLE_KEY_ECHO)
	target_include_directories(redforth PUBLIC
		${CMAKE_BINARY_DIR}
		libred/
		words/
	)
	add_dependencies(redforth codegen)

	target_link_libraries(redforth pico_stdlib)

	pico_enable_stdio_usb(redforth 1)
	pico_enable_stdio_uart(redforth 0)

	pico_add_extra_outputs(redforth)
endif()
