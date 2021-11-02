// SPDX-License-Identifier: LGPL-3.0-or-later

#include "littlefs.h"

#include "hardware/flash.h"
#include "hardware/regs/addressmap.h"
#include "hardware/sync.h"

static const unsigned RP2_BLOCK_DEVICE_SIZE = 512 * 1024;
static const unsigned RP2_BLOCK_DEVICE_OFFSET =
	PICO_FLASH_SIZE_BYTES - RP2_BLOCK_DEVICE_SIZE;

static int rp2_block_device_read(const struct lfs_config *c, lfs_block_t block,
				 lfs_off_t off, void *buf, lfs_size_t sz)
{
	unsigned char *p = (unsigned char *)(XIP_NOCACHE_NOALLOC_BASE +
					     RP2_BLOCK_DEVICE_OFFSET +
					     (block * FLASH_SECTOR_SIZE) + off);
	memcpy(buf, p, sz);

	return 0;
}

static int rp2_block_device_prog(const struct lfs_config *c, lfs_block_t block,
				  lfs_off_t off, const void *buf, lfs_size_t sz)
{
	off += RP2_BLOCK_DEVICE_OFFSET + (block * FLASH_SECTOR_SIZE);

	uint32_t status = save_and_disable_interrupts();
	flash_range_program(off, buf, sz);
	restore_interrupts(status);

	return 0;
}

static int rp2_block_device_erase(const struct lfs_config *c, lfs_block_t block)
{
	lfs_off_t off = RP2_BLOCK_DEVICE_OFFSET + (block * FLASH_SECTOR_SIZE);

	uint32_t status = save_and_disable_interrupts();
	flash_range_erase(off, FLASH_SECTOR_SIZE);
	restore_interrupts(status);

	return 0;
}

static int rp2_block_device_sync(const struct lfs_config *c)
{
	return 0;
}

const struct lfs_config rp2_littlefs_cfg = {
	.read = rp2_block_device_read,
	.prog = rp2_block_device_prog,
	.erase = rp2_block_device_erase,
	.sync = rp2_block_device_sync,

	.read_size = 1,
	.prog_size = FLASH_PAGE_SIZE,
	.block_size = FLASH_SECTOR_SIZE,
	.block_count = RP2_BLOCK_DEVICE_SIZE / FLASH_SECTOR_SIZE,
	.cache_size = FLASH_SECTOR_SIZE,
	.lookahead_size = 16,
	.block_cycles = 500,
};

lfs_t rp2_littlefs;
lfs_file_t rp2_littlefs_file;
