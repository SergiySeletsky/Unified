import { randomUUID } from "crypto";

/**
 * Represents an Immutable Unified Identifier.
 * TypeScript port compatible with the C# UnifiedId implementation.
 */
export class UnifiedId {
  /** Empty UnifiedId. */
  public static readonly Empty = new UnifiedId(0n);

  /** HEX32 Length should be 13 symbols. */
  private static readonly LENGTH = 13;

  /** Shift for (x32 >> 5 == 1) dimensions. */
  private static readonly X32_SHIFT = 5;

  /** FNV x64 Prime. */
  private static readonly PRIME = 1099511628211n;

  /** FNV x64 Offset basis. */
  private static readonly OFFSET = 14695981039346656037n;

  /** Mask for 64-bit unsigned range. */
  private static readonly UINT64_MAX = 0xFFFFFFFFFFFFFFFFn;

  /** Set of symbols used for numeric dimensions encoding. */
  private static readonly SYMBOLS = [
    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F",
    "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
  ] as const;

  /** Immutable hash value (unsigned 64-bit stored as BigInt). */
  private readonly hash: bigint;

  /**
   * Creates a new UnifiedId.
   * @param value - A bigint (unsigned 64-bit), number, or HEX32 string.
   */
  constructor(value: bigint | number | string) {
    if (typeof value === "string") {
      this.hash = UnifiedId.parseToHash(value);
    } else if (typeof value === "number") {
      // Treat as signed 64-bit integer reinterpreted as unsigned
      this.hash = BigInt.asUintN(64, BigInt(value));
    } else {
      this.hash = BigInt.asUintN(64, value);
    }
  }

  // ─── Factory Methods ───────────────────────────────────────────────

  /** Generate a new UnifiedId based on a random UUID. */
  public static newId(): UnifiedId {
    return UnifiedId.fromGuid(randomUUID());
  }

  /** Generate a UnifiedId from a UUID string. */
  public static fromGuid(guid: string): UnifiedId {
    if (!guid || guid === "00000000-0000-0000-0000-000000000000") {
      throw new RangeError("Argument 'guid' should not be empty.");
    }
    return UnifiedId.fromBytes(UnifiedId.guidToBytes(guid));
  }

  /** Generate a UnifiedId from a byte array (Uint8Array or Buffer). */
  public static fromBytes(bytes: Uint8Array): UnifiedId {
    if (!bytes) {
      throw new TypeError("Argument 'bytes' must not be null or undefined.");
    }
    if (bytes.length === 0) {
      throw new RangeError("Argument 'bytes' should not be empty.");
    }
    return new UnifiedId(UnifiedId.newHash(bytes));
  }

  /** Generate a UnifiedId from a uint64 bigint. */
  public static fromUInt64(number: bigint): UnifiedId {
    if (number === 0n) {
      throw new RangeError("Argument 'number' should not be 0.");
    }
    const bytes = new Uint8Array(8);
    let n = BigInt.asUintN(64, number);
    for (let i = 0; i < 8; i++) {
      bytes[i] = Number(n & 0xFFn);
      n >>= 8n;
    }
    return UnifiedId.fromBytes(bytes);
  }

  /** Generate a UnifiedId from a signed int64 bigint. */
  public static fromInt64(number: bigint): UnifiedId {
    if (number === 0n) {
      throw new RangeError("Argument 'number' should not be 0.");
    }
    const unsigned = BigInt.asUintN(64, number);
    const bytes = new Uint8Array(8);
    let n = unsigned;
    for (let i = 0; i < 8; i++) {
      bytes[i] = Number(n & 0xFFn);
      n >>= 8n;
    }
    return UnifiedId.fromBytes(bytes);
  }

  /** Generate a UnifiedId from a UTF-8 string. */
  public static fromString(text: string): UnifiedId {
    if (!text || text.trim().length === 0) {
      throw new TypeError("Argument 'text' must not be null or empty.");
    }
    const bytes = new TextEncoder().encode(text);
    return UnifiedId.fromBytes(bytes);
  }

  // ─── Parse ─────────────────────────────────────────────────────────

  /**
   * Parse a HEX32 string to a UnifiedId. Throws on invalid input.
   */
  public static parse(hex: string): UnifiedId {
    return new UnifiedId(UnifiedId.parseToHash(hex));
  }

  /**
   * Try to parse a HEX32 string to a UnifiedId.
   * Returns the UnifiedId on success, or null on failure.
   */
  public static tryParse(hex: string): UnifiedId | null {
    if (!hex || hex.trim().length === 0) {
      return null;
    }

    hex = hex.toUpperCase();

    if (hex.length !== UnifiedId.LENGTH) {
      return null;
    }

    if (hex === UnifiedId.Empty.toString()) {
      return UnifiedId.Empty;
    }

    const firstIndex = UnifiedId.SYMBOLS.indexOf(hex[0] as any);
    if (firstIndex < 0 || firstIndex >= 16) {
      return null;
    }

    for (const ch of hex) {
      if (!UnifiedId.SYMBOLS.includes(ch as any)) {
        return null;
      }
    }

    const decoded = UnifiedId.decode(hex);
    return new UnifiedId(decoded);
  }

  // ─── Partition Methods ─────────────────────────────────────────────

  /**
   * Get uniform virtual partition for this UnifiedId.
   * @param length Partitioning key length (1..12). Default 3 → 16K partitions (000-FVV).
   */
  public partitionKey(length: number = 3): string {
    if (length <= 0 || length >= UnifiedId.LENGTH) {
      throw new RangeError(
        `Argument 'length' is out of range, allowed value from 1 to 12.`
      );
    }
    return UnifiedId.newHex(this.hash, length);
  }

  /**
   * Get fixed-string virtual partition for this UnifiedId.
   * @param count Partition count. Default 9999.
   */
  public partitionNumberAsString(count: number = 9999): string {
    const partition = this.partitionNumber(BigInt(count));
    const digits = Math.floor(Math.log10(count)) + 1;
    return partition.toString().padStart(digits, "0");
  }

  /**
   * Get numbered virtual partition for this UnifiedId.
   * @param count Partition count (1..65535). Default 32768.
   */
  public partitionNumber(count: bigint = 32768n): bigint {
    if (count === 0n || count > 65535n) {
      throw new RangeError(
        `Argument 'count' is out of range, allowed value from 1 to 65535.`
      );
    }
    return this.hash / (UnifiedId.UINT64_MAX / count);
  }

  // ─── Conversion ────────────────────────────────────────────────────

  /** Returns the HEX32 string representation. */
  public toString(): string {
    return UnifiedId.newHex(this.hash);
  }

  /** Returns the unsigned 64-bit hash as bigint. */
  public toUInt64(): bigint {
    return this.hash;
  }

  /**
   * Returns the signed 64-bit representation as bigint.
   * Equivalent to reinterpreting the unsigned bits as signed.
   */
  public toInt64(): bigint {
    return BigInt.asIntN(64, this.hash);
  }

  /** Returns a JSON-friendly string (HEX32). */
  public toJSON(): string {
    return this.toString();
  }

  // ─── Equality / Comparison ─────────────────────────────────────────

  /** Check equality with another UnifiedId, bigint, or string. */
  public equals(other: UnifiedId | bigint | string): boolean {
    if (other instanceof UnifiedId) {
      return this.hash === other.hash;
    }
    if (typeof other === "bigint") {
      return this.hash === BigInt.asUintN(64, other);
    }
    if (typeof other === "string") {
      return this.toString() === other;
    }
    return false;
  }

  /**
   * Compare with another UnifiedId.
   * Returns -1, 0, or 1.
   */
  public compareTo(other: UnifiedId): number {
    if (this.hash < other.hash) return -1;
    if (this.hash > other.hash) return 1;
    return 0;
  }

  /** Clone this UnifiedId (returns a new instance with the same hash). */
  public clone(): UnifiedId {
    return new UnifiedId(this.hash);
  }

  // ─── Private Helpers ───────────────────────────────────────────────

  /** Internal parse that returns the raw hash bigint. */
  private static parseToHash(hex: string): bigint {
    if (!hex || hex.trim().length === 0) {
      return 0n;
    }

    if (hex === UnifiedId.Empty.toString()) {
      return 0n;
    }

    hex = hex.toUpperCase();

    if (hex.length !== UnifiedId.LENGTH) {
      throw new Error(
        `Argument 'hex'(${hex}) should have length of ${UnifiedId.LENGTH} symbols, actual length is ${hex.length} symbols.`
      );
    }

    const firstIndex = UnifiedId.SYMBOLS.indexOf(hex[0] as any);
    if (firstIndex < 0 || firstIndex >= 16) {
      throw new Error(
        `Argument 'hex'(${hex}) should contain only allowed capital symbols from '0' to 'F' for first symbol.`
      );
    }

    for (const ch of hex) {
      if (!UnifiedId.SYMBOLS.includes(ch as any)) {
        throw new Error(
          `Argument 'hex'(${hex}) should contain only allowed capital symbols from '0' to 'V'.`
        );
      }
    }

    return UnifiedId.decode(hex);
  }

  /**
   * Generate x64 FNV-1a hash from bytes.
   * Matches the C# implementation exactly (XOR-then-multiply, masked to 64 bits).
   */
  private static newHash(bytes: Uint8Array): bigint {
    let fnv = UnifiedId.OFFSET;

    for (const byte of bytes) {
      fnv ^= BigInt(byte);
      fnv = BigInt.asUintN(64, fnv * UnifiedId.PRIME);
    }

    return fnv;
  }

  /** Encode a hash to a HEX32 string. */
  private static newHex(hash: bigint, length: number = UnifiedId.LENGTH): string {
    if (hash === 0n) {
      return "0000000000000";
    }

    const hex: string[] = new Array(length);
    for (let grade = 0; grade < length; grade++) {
      const shift = UnifiedId.X32_SHIFT * (UnifiedId.LENGTH - grade - 1);
      const slice = hash >> BigInt(shift);
      const index = Number(slice & 31n);
      hex[grade] = UnifiedId.SYMBOLS[index];
    }

    return hex.join("");
  }

  /** Decode a HEX32 string back to a hash. */
  private static decode(hex: string): bigint {
    let decoded = 0n;
    for (let grade = 0; grade < UnifiedId.LENGTH; grade++) {
      const index = BigInt(UnifiedId.SYMBOLS.indexOf(hex[grade] as any));
      const shift = UnifiedId.X32_SHIFT * (UnifiedId.LENGTH - grade - 1);
      const slice = index << BigInt(shift);
      decoded += slice;
    }
    return decoded;
  }

  /**
   * Convert a UUID string to a byte array matching .NET Guid.ToByteArray() layout.
   * .NET Guid has a mixed-endian layout:
   *   bytes[0..3]  = first group  (little-endian)
   *   bytes[4..5]  = second group (little-endian)
   *   bytes[6..7]  = third group  (little-endian)
   *   bytes[8..15] = last two groups (big-endian / as-is)
   */
  private static guidToBytes(guid: string): Uint8Array {
    const hex = guid.replace(/-/g, "");
    if (hex.length !== 32) {
      throw new Error(`Invalid GUID format: ${guid}`);
    }

    const raw = new Uint8Array(16);
    for (let i = 0; i < 16; i++) {
      raw[i] = parseInt(hex.substring(i * 2, i * 2 + 2), 16);
    }

    // Reorder to match .NET Guid.ToByteArray() mixed-endian layout
    const bytes = new Uint8Array(16);
    // Group 1 (4 bytes) - little-endian
    bytes[0] = raw[3];
    bytes[1] = raw[2];
    bytes[2] = raw[1];
    bytes[3] = raw[0];
    // Group 2 (2 bytes) - little-endian
    bytes[4] = raw[5];
    bytes[5] = raw[4];
    // Group 3 (2 bytes) - little-endian
    bytes[6] = raw[7];
    bytes[7] = raw[6];
    // Groups 4 & 5 (8 bytes) - big-endian (as-is)
    bytes[8] = raw[8];
    bytes[9] = raw[9];
    bytes[10] = raw[10];
    bytes[11] = raw[11];
    bytes[12] = raw[12];
    bytes[13] = raw[13];
    bytes[14] = raw[14];
    bytes[15] = raw[15];

    return bytes;
  }
}
