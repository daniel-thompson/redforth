// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_RP2_WORDS_H_
#define RF_RP2_WORDS_H_

QNATIVE(REBOOT)
#undef  LINK
#define LINK REBOOT
	do_REBOOT();
	NEXT();

#endif /* RF_WORDS_STDC_H_ */
