import hmac
import hashlib

## use this to sign a message. be careful as the required key is different for every function
def sign_message(sender_id: str, key: str, secret: str) -> str:
    """
    Create a secure HMAC signature for a message using a secret key.
    Returns a hex string.
    """
    signature = hmac.new(
        key=secret.encode("utf-8"),
        msg=(sender_id + key).encode("utf-8"),
        digestmod=hashlib.sha256  # secure choice
    )
    return signature.hexdigest()

def verify_signature(sender_id: str, key: str, secret: str, signature: str) -> bool:
    """
    Verify a message against a given signature.
    Returns True if valid, False otherwise.
    """
    expected_sig = sign_message(sender_id, key, secret)
    return hmac.compare_digest(expected_sig, signature)
