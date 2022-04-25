const Request = window.Request
const Headers = window.Headers
const fetch = window.fetch

class Validation {
    isValid(domainName) {
        const regex = /(?=^.{4,253}$)(^((?!-)[a-zA-Z0-9-]{1,63}(?<!-)\.)+[a-zA-Z]{2,63}$)/;
        return domainName.match(regex);
    }
}

class Domain {
    constructor(response) {
        this.name = response.request.name;
        this.ip = response.request.ip;
        this.host = response.request.hostedAt;
        this.whois = response.request.whoIs;
    }
}

class Api {
  async request (method, url, body) {
        if (body) {
          body = JSON.stringify(body)
        }

        const request = new Request('/api/' + url, {
          method: method,
          body: body,
          credentials: 'same-origin',
          headers: new Headers({
            'Accept': 'application/json',
            'Content-Type': 'application/json'
          })
        })

        const resp = await fetch(request)

        if (!resp.ok && resp.status !== 400) {
            throw Error(resp.statusText)
        }

        const jsonResult = await resp.json()

        if (resp.status === 400) {
            jsonResult.requestStatus = 400
        }

        return jsonResult
    }

    async getDomain(domainName) {
        try {
            var result = await this.request('GET', `domain/${domainName}`);
            return { request: result, success: true }
        } catch (ex) {
            if (ex.status === 400) {
                window.alert(ex.messages.invalidDomain)
                return { success: false };
            }

            if (ex.status === 500) {
                window.alert(ex.messages.errorServer)
                return { success: false };
            }
        }
    }
}

const api = new Api()
const validation = new Validation()

var callback = () => {
  const btn = document.getElementById('btn-search')
    const txtSearch = document.getElementById('txt-search')
    const dName = document.querySelector('#name-results')
    const ip = document.querySelector('#ip-results')
    const host = document.querySelector('#host-results')
    const whois = document.querySelector('#whois-results')

  if (btn) {
      btn.onclick = async () => {

          var domainName = txtSearch.value
          var isValid = validation.isValid(domainName)
          if (!isValid) {
              window.alert("Nome de domínio inválido")
          } else {
              var domain = await api.getDomain(domainName)
                  .then(function (response) {
                      return new Domain(response);
                  })

              dName.setAttribute('value', `${domain.name}`)
              ip.setAttribute('value', `${domain.ip}`)
              host.setAttribute('value', `${domain.host}`)
              whois.innerText = domain.whois
          }
      }
    }
  }


if (document.readyState === 'complete' || (document.readyState !== 'loading' && !document.documentElement.doScroll)) {
  callback()
} else {
  document.addEventListener('DOMContentLoaded', callback)
}
