FROM elasticsearch:8.19.12

USER root

# Get the NIH Root certificates and install them so we work on VPN
# Download from https://ocio.nih.gov/Smartcard/Pages/PKI_chain.aspx
RUN mkdir -p /usr/local/share/ca-certificates/nih \
    && curl -o /usr/local/share/ca-certificates/nih/NIH-DPKI-ROOT-1A-base64.crt https://ocio.nih.gov/Smartcard/Documents/Certificates/NIH-DPKI-ROOT-1A-base64.cer \
    && curl -o /usr/local/share/ca-certificates/nih/NIH-DPKI-CA-1A-base64.crt https://ocio.nih.gov/Smartcard/Documents/Certificates/NIH-DPKI-CA-1A-base64.cer \
    && update-ca-certificates

USER elasticsearch