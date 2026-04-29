const textDecoder = new TextDecoder("utf-8");

export class BinaryReader {
    private readonly _dataView: DataView;
    private _currentIndex = 0;

    constructor(data: Uint8Array) {
        this._dataView = new DataView(data.buffer, data.byteOffset, data.byteLength);
    }

    readInt32(): number {
        const value = this._dataView.getInt32(this._currentIndex, true);
        this._currentIndex += 4;
        return value;
    }

    readUint8(): number {
        const value = this._dataView.getUint8(this._currentIndex);
        this._currentIndex += 1;
        return value;
    }

    readFloat64(): number {
        const value = this._dataView.getFloat64(this._currentIndex, true);
        this._currentIndex += 8;
        return value;
    }

    readString(): string {
        const length = this.readInt32();
        if (length === 0) {
            return '';
        }

        const buffer = new Uint8Array(this._dataView.buffer, this._dataView.byteOffset + this._currentIndex, length);
        const result = textDecoder.decode(buffer);
        this._currentIndex += length;
        return result;
    }

    readBoolean(): boolean {
        return this.readUint8() > 0;
    }

    readUint8Array(length: number): Uint8Array {
        const data = new Uint8Array(this._dataView.buffer, this._dataView.byteOffset + this._currentIndex, length);
        this._currentIndex += length;
        return data;
    }
}
