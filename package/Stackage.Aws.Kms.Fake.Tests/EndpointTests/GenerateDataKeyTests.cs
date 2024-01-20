﻿namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class GenerateDataKeyTests
{
   /*
aws kms generate-data-key --key-id 1899be22-cb31-4f3f-b01b-fc41aa0a524c --key-spec AES_256  --endpoint-url http://localhost:4566 --region eu-central-1

{"KeyId": "683616ef-d857-49ef-b63e-4480f2dc4cd1"}
{"KeyId": "arn:aws:kms:eu-central-1:000000000000:key/683616ef-d857-49ef-b63e-4480f2dc4cd1", "Plaintext": "67KjrTe9EQ7yz5YD84FpggaYWng+y4g1pL1ZwT4fD8w=", "CiphertextBlob": "NjgzNjE2ZWYtZDg1Ny00OWVmLWI2M2UtNDQ4MGYyZGM0Y2QxTLPNDOzLk8jedYC+sYH0B95s0bvd0EYdB3Tbs27Xyn28IgYaKXLUoRNZRk/f+VK9rcrwGrETddRrng+jA3ADshpc3KLzcWmJwAsDSAbIZgM="}

{
    "CiphertextBlob": "MTg5OWJlMjItY2IzMS00ZjNmLWIwMWItZmM0MWFhMGE1MjRjRx1Kj7Z8K83Ycb8FKiKenei8hCjIN0N02POFOilniCVJxgCyrhr2Zu4ats6H3HwnR8UKvBli0Jg7EXlTrckr5CyV8QIB1Cbad28Pq7CHmFY=",
    "Plaintext": "qew0WyrLfL0f5UqCqfFTTN/Dwkv3lWUB42yz6+zNu7o=",
    "KeyId": "arn:aws:kms:eu-central-1:000000000000:key/1899be22-cb31-4f3f-b01b-fc41aa0a524c"
}
    */

   // TODO: WHat about aliases? Can use alias in place of key-id in encrypt and generate-data-key
   /*
    *  create-alias
--alias-name <value>
--target-key-id <value>
    */
}