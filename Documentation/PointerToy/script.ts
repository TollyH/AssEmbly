const registers: {[byte: number]: string;} = {
    0x00: "rpo",
    0x01: "rso",
    0x02: "rsb",
    0x03: "rsf",
    0x04: "rrv",
    0x05: "rfp",
    0x06: "rg0",
    0x07: "rg1",
    0x08: "rg2",
    0x09: "rg3",
    0x0A: "rg4",
    0x0B: "rg5",
    0x0C: "rg6",
    0x0D: "rg7",
    0x0E: "rg8",
    0x0F: "rg9"
};

const multipliers: {[bits: number]: string;} = {
    0b000: "",
    0b001: " * 2",
    0b010: " * 4",
    0b011: " * 8",
    0b100: " * 16",
    0b101: " * 32",
    0b110: " * 64",
    0b111: " * 128"
};

const constantSignBit = 1n << 63n;

let firstByte = 0;
let constant = 0n;
let registerByte = 0;

let firstByteBits : HTMLElement[] = [];
let constantBits : HTMLElement[] = [];
let registerBits : HTMLElement[] = [];

let pointerAssembly : HTMLElement;
let pointerMode : HTMLElement;
let pointerReadSize : HTMLElement;
let pointerLength : HTMLElement;
let pointerAssembledBytes : HTMLElement;

let firstByteHex : HTMLInputElement;
let constantHex : HTMLInputElement;
let registerHex : HTMLInputElement;

let constantPageSection: HTMLElement;
let registerPageSection: HTMLElement;

function twosComplementNegate(value: bigint) {
    return (value ^ 0xFFFFFFFFFFFFFFFFn) + 1n;
}

function formatByte(byte: number | bigint) {
    return byte.toString(16).toUpperCase().padStart(2, '0');
}

function _decodePointerRegister() {
    let registerText = "";

    if ((registerByte & 0b10000000) !== 0) {
        registerText += "-";
    }

    registerText += registers[registerByte & 0b1111];
    registerText += multipliers[(registerByte >> 4) & 0b111];

    return registerText;
}

function decodePointer() {
    let pointerText = "";
    let assembledBytesText = formatByte(firstByte);

    // Decode pointer size
    let sizeBits = (firstByte >> 4) & 0b11;
    if (sizeBits === 0b00) {
        pointerReadSize.textContent = "8 bytes (64 bits)";
    }
    else if (sizeBits === 0b01) {
        pointerText += "D";
        pointerReadSize.textContent = "4 bytes (32 bits)";
    }
    else if (sizeBits === 0b10) {
        pointerText += "W";
        pointerReadSize.textContent = "2 bytes (16 bits)";
    }
    else {
        pointerText += "B";
        pointerReadSize.textContent = "1 byte (8 bits)";
    }

    pointerText += "*";
    pointerText += registers[firstByte & 0b1111];

    // Decoder pointer mode
    let modeBits = (firstByte >> 6) & 0b11;
    if (modeBits === 0b00) {
        pointerMode.textContent = "No Displacement";
        pointerLength.textContent = "1 byte";
        constantPageSection.classList.add("hidden");
        registerPageSection.classList.add("hidden");
    }
    else {
        pointerText += "[";
        if (modeBits === 0b01) {
            if ((constant & constantSignBit) === 0n) {
                pointerText += constant;
            }
            else {
                pointerText += "-" + twosComplementNegate(constant);
            }
            let constantValue = constant;
            for (let i = 0; i < 8; i++, constantValue >>= 8n) {
                assembledBytesText += " " + formatByte(constantValue & 0xFFn);
            }
            pointerMode.textContent = "Constant";
            pointerLength.textContent = "9 bytes";
            constantPageSection.classList.remove("hidden");
            registerPageSection.classList.add("hidden");
        }
        else if (modeBits === 0b10) {
            pointerText += _decodePointerRegister();
            assembledBytesText += " " + formatByte(registerByte);
            pointerMode.textContent = "Register";
            pointerLength.textContent = "2 bytes";
            constantPageSection.classList.add("hidden");
            registerPageSection.classList.remove("hidden");
        }
        else {
            pointerText += _decodePointerRegister();
            if ((constant & constantSignBit) === 0n) {
                pointerText += " + " + constant;
            }
            else {
                pointerText += " - " + twosComplementNegate(constant);
            }
            let constantValue = constant;
            for (let i = 0; i < 8; i++, constantValue >>= 8n) {
                assembledBytesText += " " + formatByte(constantValue & 0xFFn);
            }
            assembledBytesText += " " + formatByte(registerByte);
            pointerMode.textContent = "Constant and Register";
            pointerLength.textContent = "10 bytes";
            constantPageSection.classList.remove("hidden");
            registerPageSection.classList.remove("hidden");
        }
        pointerText += "]";
    }

    pointerAssembly.textContent = pointerText;
    pointerAssembledBytes.textContent = assembledBytesText;
}

function updateFirstByteHex() {
    firstByteHex.value = formatByte(firstByte);
}

function updateConstantHex() {
    constantHex.value = constant.toString(16).toUpperCase().padStart(16, '0');
}

function updateRegisterHex() {
    registerHex.value = formatByte(registerByte);
}

function updateFirstByteBits() {
    let firstByteValue = firstByte;

    for (let i = 0; i < 8; i++, firstByteValue >>= 1) {
        let element = firstByteBits[8 - i - 1];

        if ((firstByteValue & 1) != 0) {
            element.classList.add("set");
            element.textContent = "1";
        }
        else {
            element.classList.remove("set");
            element.textContent = "0";
        }
    }
}

function updateConstantBits() {
    let constantValue = constant;

    for (let i = 0; i < 64; i++, constantValue >>= 1n) {
        let element = constantBits[64 - i - 1];
        
        if ((constantValue & 1n) != 0n) {
            element.classList.add("set");
            element.textContent = "1";
        }
        else {
            element.classList.remove("set");
            element.textContent = "0";
        }
    }
}

function updateRegisterBits() {
    let registerByteValue = registerByte;

    for (let i = 0; i < 8; i++, registerByteValue >>= 1) {
        let element = registerBits[8 - i - 1];
        
        if ((registerByteValue & 1) != 0) {
            element.classList.add("set");
            element.textContent = "1";
        }
        else {
            element.classList.remove("set");
            element.textContent = "0";
        }
    }
}

function onFirstByteBitClick(bit: number, index: number) {
    let element = firstByteBits[index];

    if (element.classList.contains("set")) {
        element.classList.remove("set");
        element.textContent = "0";
        firstByte &= ~(1 << bit);
    }
    else {
        element.classList.add("set");
        element.textContent = "1";
        firstByte |= 1 << bit;
    }

    updateFirstByteHex();
    decodePointer();
}

function onConstantBitClick(bit: number, index: number) {
    let element = constantBits[index];

    if (element.classList.contains("set")) {
        element.classList.remove("set");
        element.textContent = "0";
        constant &= ~(1n << BigInt(bit));
    }
    else {
        element.classList.add("set");
        element.textContent = "1";
        constant |= 1n << BigInt(bit);
    }

    updateConstantHex();
    decodePointer();
}

function onRegisterBitClick(bit: number, index: number) {
    let element = registerBits[index];

    if (element.classList.contains("set")) {
        element.classList.remove("set");
        element.textContent = "0";
        registerByte &= ~(1 << bit);
    }
    else {
        element.classList.add("set");
        element.textContent = "1";
        registerByte |= 1 << bit;
    }

    updateRegisterHex();
    decodePointer();
}

function onFirstByteHexEdit() {
    firstByte = parseInt(firstByteHex.value ?? "", 16) & 0xFF;

    updateFirstByteBits();
    updateFirstByteHex();
    decodePointer();
}

function onConstantHexEdit() {
    constant = BigInt("0x" + constantHex.value ?? "") & 0xFFFFFFFFFFFFFFFFn;

    updateConstantBits();
    updateConstantHex();
    decodePointer();
}

function onRegisterHexEdit() {
    registerByte = parseInt(registerHex.value ?? "", 16) & 0xFF;

    updateRegisterBits();
    updateRegisterHex();
    decodePointer();
}

window.onload = function() {
    pointerAssembly = document.getElementById("pointer-assembly-text")!;
    pointerMode = document.getElementById("pointer-mode")!;
    pointerReadSize = document.getElementById("pointer-read-size")!;
    pointerLength = document.getElementById("pointer-length")!;
    pointerAssembledBytes = document.getElementById("pointer-assembly-bytes")!;

    let firstByteBitCollection = document.getElementById("pointer-first-byte-bits")!.children;
    let constantBitCollection = document.getElementById("pointer-constant-bits")!.children;
    let registerBitCollection = document.getElementById("pointer-register-bits")!.children;

    for (let i = 0; i < firstByteBitCollection.length; i++) {
        let element = firstByteBitCollection[i];
        if (!(element instanceof HTMLElement)) {
            continue;
        }
        let bit = firstByteBitCollection.length - i - 1;
        element.onclick = function() { onFirstByteBitClick(bit, i); };
        firstByteBits.push(element);
    }
    for (let i = 0; i < constantBitCollection.length; i++) {
        let element = constantBitCollection[i];
        if (!(element instanceof HTMLElement)) {
            continue;
        }
        let bit = constantBitCollection.length - i - 1;
        element.onclick = function() { onConstantBitClick(bit, i); };
        constantBits.push(element);
    }
    for (let i = 0; i < registerBitCollection.length; i++) {
        let element = registerBitCollection[i];
        if (!(element instanceof HTMLElement)) {
            continue;
        }
        let bit = registerBitCollection.length - i - 1;
        element.onclick = function() { onRegisterBitClick(bit, i); };
        registerBits.push(element);
    }

    firstByteHex = <HTMLInputElement>document.getElementById("pointer-first-byte-hex")!;
    constantHex = <HTMLInputElement>document.getElementById("pointer-constant-hex")!;
    registerHex = <HTMLInputElement>document.getElementById("pointer-register-hex")!;

    firstByteHex.onchange = onFirstByteHexEdit;
    constantHex.onchange = onConstantHexEdit;
    registerHex.onchange = onRegisterHexEdit;

    constantPageSection = document.getElementById("pointer-constant")!;
    registerPageSection = document.getElementById("pointer-register-byte")!;

    updateFirstByteHex();
    updateConstantHex();
    updateRegisterHex();

    decodePointer();
}
