import * as request from 'request-promise-native';

import { cloudServicesConfig } from '../configs/cloudServicesConfig';
import { Tag } from './tag';

export class AnnotationService {

    public async annotate(imageUri: string, maxResults: number) {
        const baseUrl = `https://vision.googleapis.com/v1/images:annotate?key=${cloudServicesConfig.visionApiKey}`;

        const body = JSON.stringify({
            "requests": [{
                "image": {
                    "source": {
                        "imageUri": imageUri
                    }
                },
                "features": [{
                    "type": "LABEL_DETECTION",
                    "maxResults": maxResults
                }]
            }]
        });

        var options = {
            uri: baseUrl,
            body: body
        };

        const response = JSON.parse(await request.post(options));
        const tags = response.responses[0].labelAnnotations as Tag[];

        return !tags
            ? []
            : tags.map(x => x.description);
    }
}