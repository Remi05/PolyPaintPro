import { CloudFunction } from "firebase-functions";
import { ObjectMetadata } from "firebase-functions/lib/providers/storage";
import { DeltaSnapshot } from "firebase-functions/lib/providers/database";

export interface HasTriggers {
    /// Should register triggers 
    getTriggers(): CloudFunction<ObjectMetadata | DeltaSnapshot>[] | CloudFunction<ObjectMetadata | DeltaSnapshot>;
}