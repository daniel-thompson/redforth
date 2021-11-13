// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_RP2_WORDS_H_
#define RF_RP2_WORDS_H_

#define EXPORT_FUNCTION(fname) CONSTANT("FN-" #fname, FN_##fname, (uintptr_t) &(fname))

CONSTANT("LFS", LFS, (uintptr_t) &rp2_littlefs)
#undef  LINK
#define LINK LFS

CONSTANT("LFS_FILE_SIZE", LFS_FILE_SIZE, sizeof(lfs_file_t))
#undef  LINK
#define LINK LFS_FILE_SIZE

CONSTANT("LFS_O_RDONLY", LFS_O_RDONLY, LFS_O_RDONLY)
#undef  LINK
#define LINK LFS_O_RDONLY

CONSTANT("LFS_O_WRONLY", LFS_O_WRONLY, LFS_O_WRONLY)
#undef  LINK
#define LINK LFS_O_WRONLY

CONSTANT("LFS_O_CREAT", LFS_O_CREAT, LFS_O_CREAT)
#undef  LINK
#define LINK LFS_O_CREAT

CONSTANT("LFS_O_TRUNC", LFS_O_TRUNC, LFS_O_TRUNC)
#undef  LINK
#define LINK LFS_O_TRUNC

CONSTANT("LFS_O_APPEND", LFS_O_APPEND, LFS_O_APPEND)
#undef  LINK
#define LINK LFS_O_APPEND

QNATIVE(MOUNT)
#undef  LINK
#define LINK MOUNT
	PUSH((cell_t) { .u = lfs_mount(&rp2_littlefs, &rp2_littlefs_cfg) });
	NEXT();

QNATIVE(FORMAT)
#undef  LINK
#define LINK FORMAT
	PUSH((cell_t) { .u = lfs_format(&rp2_littlefs, &rp2_littlefs_cfg) });
	NEXT();

EXPORT_FUNCTION(lfs_file_open)
#undef  LINK
#define LINK FN_lfs_file_open

EXPORT_FUNCTION(lfs_file_close)
#undef  LINK
#define LINK FN_lfs_file_close

EXPORT_FUNCTION(lfs_file_read)
#undef  LINK
#define LINK FN_lfs_file_read

EXPORT_FUNCTION(lfs_file_write)
#undef  LINK
#define LINK FN_lfs_file_write

EXPORT_FUNCTION(exit)
#undef  LINK
#define LINK FN_exit

QNATIVE(REBOOT)
#undef  LINK
#define LINK REBOOT
	do_REBOOT();
	NEXT();

#endif /* RF_RP2_WORDS_H_ */
