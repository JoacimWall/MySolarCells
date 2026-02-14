function t(t,e,s,i){var r,o=arguments.length,a=o<3?e:null===i?i=Object.getOwnPropertyDescriptor(e,s):i;if("object"==typeof Reflect&&"function"==typeof Reflect.decorate)a=Reflect.decorate(t,e,s,i);else for(var n=t.length-1;n>=0;n--)(r=t[n])&&(a=(o<3?r(a):o>3?r(e,s,a):r(e,s))||a);return o>3&&a&&Object.defineProperty(e,s,a),a}"function"==typeof SuppressedError&&SuppressedError;
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const e=globalThis,s=e.ShadowRoot&&(void 0===e.ShadyCSS||e.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,i=Symbol(),r=new WeakMap;let o=class{constructor(t,e,s){if(this._$cssResult$=!0,s!==i)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(s&&void 0===t){const s=void 0!==e&&1===e.length;s&&(t=r.get(e)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),s&&r.set(e,t))}return t}toString(){return this.cssText}};const a=(t,...e)=>{const s=1===t.length?t[0]:e.reduce((e,s,i)=>e+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+t[i+1],t[0]);return new o(s,t,i)},n=s?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let e="";for(const s of t.cssRules)e+=s.cssText;return(t=>new o("string"==typeof t?t:t+"",void 0,i))(e)})(t):t,{is:d,defineProperty:h,getOwnPropertyDescriptor:l,getOwnPropertyNames:c,getOwnPropertySymbols:p,getPrototypeOf:u}=Object,_=globalThis,v=_.trustedTypes,g=v?v.emptyScript:"",y=_.reactiveElementPolyfillSupport,f=(t,e)=>t,$={toAttribute(t,e){switch(e){case Boolean:t=t?g:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,e){let s=t;switch(e){case Boolean:s=null!==t;break;case Number:s=null===t?null:Number(t);break;case Object:case Array:try{s=JSON.parse(t)}catch(t){s=null}}return s}},m=(t,e)=>!d(t,e),b={attribute:!0,type:String,converter:$,reflect:!1,useDefault:!1,hasChanged:m};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */Symbol.metadata??=Symbol("metadata"),_.litPropertyMetadata??=new WeakMap;let x=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??=[]).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=b){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const s=Symbol(),i=this.getPropertyDescriptor(t,s,e);void 0!==i&&h(this.prototype,t,i)}}static getPropertyDescriptor(t,e,s){const{get:i,set:r}=l(this.prototype,t)??{get(){return this[e]},set(t){this[e]=t}};return{get:i,set(e){const o=i?.call(this);r?.call(this,e),this.requestUpdate(t,o,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??b}static _$Ei(){if(this.hasOwnProperty(f("elementProperties")))return;const t=u(this);t.finalize(),void 0!==t.l&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(f("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(f("properties"))){const t=this.properties,e=[...c(t),...p(t)];for(const s of e)this.createProperty(s,t[s])}const t=this[Symbol.metadata];if(null!==t){const e=litPropertyMetadata.get(t);if(void 0!==e)for(const[t,s]of e)this.elementProperties.set(t,s)}this._$Eh=new Map;for(const[t,e]of this.elementProperties){const s=this._$Eu(t,e);void 0!==s&&this._$Eh.set(s,t)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const t of s)e.unshift(n(t))}else void 0!==t&&e.push(n(t));return e}static _$Eu(t,e){const s=e.attribute;return!1===s?void 0:"string"==typeof s?s:"string"==typeof t?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){this._$ES=new Promise(t=>this.enableUpdating=t),this._$AL=new Map,this._$E_(),this.requestUpdate(),this.constructor.l?.forEach(t=>t(this))}addController(t){(this._$EO??=new Set).add(t),void 0!==this.renderRoot&&this.isConnected&&t.hostConnected?.()}removeController(t){this._$EO?.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const s of e.keys())this.hasOwnProperty(s)&&(t.set(s,this[s]),delete this[s]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return((t,i)=>{if(s)t.adoptedStyleSheets=i.map(t=>t instanceof CSSStyleSheet?t:t.styleSheet);else for(const s of i){const i=document.createElement("style"),r=e.litNonce;void 0!==r&&i.setAttribute("nonce",r),i.textContent=s.cssText,t.appendChild(i)}})(t,this.constructor.elementStyles),t}connectedCallback(){this.renderRoot??=this.createRenderRoot(),this.enableUpdating(!0),this._$EO?.forEach(t=>t.hostConnected?.())}enableUpdating(t){}disconnectedCallback(){this._$EO?.forEach(t=>t.hostDisconnected?.())}attributeChangedCallback(t,e,s){this._$AK(t,s)}_$ET(t,e){const s=this.constructor.elementProperties.get(t),i=this.constructor._$Eu(t,s);if(void 0!==i&&!0===s.reflect){const r=(void 0!==s.converter?.toAttribute?s.converter:$).toAttribute(e,s.type);this._$Em=t,null==r?this.removeAttribute(i):this.setAttribute(i,r),this._$Em=null}}_$AK(t,e){const s=this.constructor,i=s._$Eh.get(t);if(void 0!==i&&this._$Em!==i){const t=s.getPropertyOptions(i),r="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==t.converter?.fromAttribute?t.converter:$;this._$Em=i;const o=r.fromAttribute(e,t.type);this[i]=o??this._$Ej?.get(i)??o,this._$Em=null}}requestUpdate(t,e,s,i=!1,r){if(void 0!==t){const o=this.constructor;if(!1===i&&(r=this[t]),s??=o.getPropertyOptions(t),!((s.hasChanged??m)(r,e)||s.useDefault&&s.reflect&&r===this._$Ej?.get(t)&&!this.hasAttribute(o._$Eu(t,s))))return;this.C(t,e,s)}!1===this.isUpdatePending&&(this._$ES=this._$EP())}C(t,e,{useDefault:s,reflect:i,wrapped:r},o){s&&!(this._$Ej??=new Map).has(t)&&(this._$Ej.set(t,o??e??this[t]),!0!==r||void 0!==o)||(this._$AL.has(t)||(this.hasUpdated||s||(e=void 0),this._$AL.set(t,e)),!0===i&&this._$Em!==t&&(this._$Eq??=new Set).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??=this.createRenderRoot(),this._$Ep){for(const[t,e]of this._$Ep)this[t]=e;this._$Ep=void 0}const t=this.constructor.elementProperties;if(t.size>0)for(const[e,s]of t){const{wrapped:t}=s,i=this[e];!0!==t||this._$AL.has(e)||void 0===i||this.C(e,void 0,s,i)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),this._$EO?.forEach(t=>t.hostUpdate?.()),this.update(e)):this._$EM()}catch(e){throw t=!1,this._$EM(),e}t&&this._$AE(e)}willUpdate(t){}_$AE(t){this._$EO?.forEach(t=>t.hostUpdated?.()),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&=this._$Eq.forEach(t=>this._$ET(t,this[t])),this._$EM()}updated(t){}firstUpdated(t){}};x.elementStyles=[],x.shadowRootOptions={mode:"open"},x[f("elementProperties")]=new Map,x[f("finalized")]=new Map,y?.({ReactiveElement:x}),(_.reactiveElementVersions??=[]).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const A=globalThis,w=t=>t,S=A.trustedTypes,E=S?S.createPolicy("lit-html",{createHTML:t=>t}):void 0,T="$lit$",C=`lit$${Math.random().toFixed(9).slice(2)}$`,P="?"+C,k=`<${P}>`,D=document,O=()=>D.createComment(""),U=t=>null===t||"object"!=typeof t&&"function"!=typeof t,I=Array.isArray,M="[ \t\n\f\r]",R=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,H=/-->/g,N=/>/g,L=RegExp(`>|${M}(?:([^\\s"'>=/]+)(${M}*=${M}*(?:[^ \t\n\f\r"'\`<>=]|("|')|))|$)`,"g"),z=/'/g,j=/"/g,F=/^(?:script|style|textarea|title)$/i,W=(t=>(e,...s)=>({_$litType$:t,strings:e,values:s}))(1),B=Symbol.for("lit-noChange"),q=Symbol.for("lit-nothing"),V=new WeakMap,K=D.createTreeWalker(D,129);function Y(t,e){if(!I(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return void 0!==E?E.createHTML(e):e}const J=(t,e)=>{const s=t.length-1,i=[];let r,o=2===e?"<svg>":3===e?"<math>":"",a=R;for(let e=0;e<s;e++){const s=t[e];let n,d,h=-1,l=0;for(;l<s.length&&(a.lastIndex=l,d=a.exec(s),null!==d);)l=a.lastIndex,a===R?"!--"===d[1]?a=H:void 0!==d[1]?a=N:void 0!==d[2]?(F.test(d[2])&&(r=RegExp("</"+d[2],"g")),a=L):void 0!==d[3]&&(a=L):a===L?">"===d[0]?(a=r??R,h=-1):void 0===d[1]?h=-2:(h=a.lastIndex-d[2].length,n=d[1],a=void 0===d[3]?L:'"'===d[3]?j:z):a===j||a===z?a=L:a===H||a===N?a=R:(a=L,r=void 0);const c=a===L&&t[e+1].startsWith("/>")?" ":"";o+=a===R?s+k:h>=0?(i.push(n),s.slice(0,h)+T+s.slice(h)+C+c):s+C+(-2===h?e:c)}return[Y(t,o+(t[s]||"<?>")+(2===e?"</svg>":3===e?"</math>":"")),i]};class Z{constructor({strings:t,_$litType$:e},s){let i;this.parts=[];let r=0,o=0;const a=t.length-1,n=this.parts,[d,h]=J(t,e);if(this.el=Z.createElement(d,s),K.currentNode=this.el.content,2===e||3===e){const t=this.el.content.firstChild;t.replaceWith(...t.childNodes)}for(;null!==(i=K.nextNode())&&n.length<a;){if(1===i.nodeType){if(i.hasAttributes())for(const t of i.getAttributeNames())if(t.endsWith(T)){const e=h[o++],s=i.getAttribute(t).split(C),a=/([.?@])?(.*)/.exec(e);n.push({type:1,index:r,name:a[2],strings:s,ctor:"."===a[1]?et:"?"===a[1]?st:"@"===a[1]?it:tt}),i.removeAttribute(t)}else t.startsWith(C)&&(n.push({type:6,index:r}),i.removeAttribute(t));if(F.test(i.tagName)){const t=i.textContent.split(C),e=t.length-1;if(e>0){i.textContent=S?S.emptyScript:"";for(let s=0;s<e;s++)i.append(t[s],O()),K.nextNode(),n.push({type:2,index:++r});i.append(t[e],O())}}}else if(8===i.nodeType)if(i.data===P)n.push({type:2,index:r});else{let t=-1;for(;-1!==(t=i.data.indexOf(C,t+1));)n.push({type:7,index:r}),t+=C.length-1}r++}}static createElement(t,e){const s=D.createElement("template");return s.innerHTML=t,s}}function G(t,e,s=t,i){if(e===B)return e;let r=void 0!==i?s._$Co?.[i]:s._$Cl;const o=U(e)?void 0:e._$litDirective$;return r?.constructor!==o&&(r?._$AO?.(!1),void 0===o?r=void 0:(r=new o(t),r._$AT(t,s,i)),void 0!==i?(s._$Co??=[])[i]=r:s._$Cl=r),void 0!==r&&(e=G(t,r._$AS(t,e.values),r,i)),e}class Q{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:s}=this._$AD,i=(t?.creationScope??D).importNode(e,!0);K.currentNode=i;let r=K.nextNode(),o=0,a=0,n=s[0];for(;void 0!==n;){if(o===n.index){let e;2===n.type?e=new X(r,r.nextSibling,this,t):1===n.type?e=new n.ctor(r,n.name,n.strings,this,t):6===n.type&&(e=new rt(r,this,t)),this._$AV.push(e),n=s[++a]}o!==n?.index&&(r=K.nextNode(),o++)}return K.currentNode=D,i}p(t){let e=0;for(const s of this._$AV)void 0!==s&&(void 0!==s.strings?(s._$AI(t,s,e),e+=s.strings.length-2):s._$AI(t[e])),e++}}class X{get _$AU(){return this._$AM?._$AU??this._$Cv}constructor(t,e,s,i){this.type=2,this._$AH=q,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=s,this.options=i,this._$Cv=i?.isConnected??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return void 0!==e&&11===t?.nodeType&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=G(this,t,e),U(t)?t===q||null==t||""===t?(this._$AH!==q&&this._$AR(),this._$AH=q):t!==this._$AH&&t!==B&&this._(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>I(t)||"function"==typeof t?.[Symbol.iterator])(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==q&&U(this._$AH)?this._$AA.nextSibling.data=t:this.T(D.createTextNode(t)),this._$AH=t}$(t){const{values:e,_$litType$:s}=t,i="number"==typeof s?this._$AC(t):(void 0===s.el&&(s.el=Z.createElement(Y(s.h,s.h[0]),this.options)),s);if(this._$AH?._$AD===i)this._$AH.p(e);else{const t=new Q(i,this),s=t.u(this.options);t.p(e),this.T(s),this._$AH=t}}_$AC(t){let e=V.get(t.strings);return void 0===e&&V.set(t.strings,e=new Z(t)),e}k(t){I(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let s,i=0;for(const r of t)i===e.length?e.push(s=new X(this.O(O()),this.O(O()),this,this.options)):s=e[i],s._$AI(r),i++;i<e.length&&(this._$AR(s&&s._$AB.nextSibling,i),e.length=i)}_$AR(t=this._$AA.nextSibling,e){for(this._$AP?.(!1,!0,e);t!==this._$AB;){const e=w(t).nextSibling;w(t).remove(),t=e}}setConnected(t){void 0===this._$AM&&(this._$Cv=t,this._$AP?.(t))}}class tt{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,s,i,r){this.type=1,this._$AH=q,this._$AN=void 0,this.element=t,this.name=e,this._$AM=i,this.options=r,s.length>2||""!==s[0]||""!==s[1]?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=q}_$AI(t,e=this,s,i){const r=this.strings;let o=!1;if(void 0===r)t=G(this,t,e,0),o=!U(t)||t!==this._$AH&&t!==B,o&&(this._$AH=t);else{const i=t;let a,n;for(t=r[0],a=0;a<r.length-1;a++)n=G(this,i[s+a],e,a),n===B&&(n=this._$AH[a]),o||=!U(n)||n!==this._$AH[a],n===q?t=q:t!==q&&(t+=(n??"")+r[a+1]),this._$AH[a]=n}o&&!i&&this.j(t)}j(t){t===q?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class et extends tt{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===q?void 0:t}}class st extends tt{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==q)}}class it extends tt{constructor(t,e,s,i,r){super(t,e,s,i,r),this.type=5}_$AI(t,e=this){if((t=G(this,t,e,0)??q)===B)return;const s=this._$AH,i=t===q&&s!==q||t.capture!==s.capture||t.once!==s.once||t.passive!==s.passive,r=t!==q&&(s===q||i);i&&this.element.removeEventListener(this.name,this,s),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){"function"==typeof this._$AH?this._$AH.call(this.options?.host??this.element,t):this._$AH.handleEvent(t)}}class rt{constructor(t,e,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){G(this,t)}}const ot=A.litHtmlPolyfillSupport;ot?.(Z,X),(A.litHtmlVersions??=[]).push("3.3.2");const at=globalThis;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class nt extends x{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){const t=super.createRenderRoot();return this.renderOptions.renderBefore??=t.firstChild,t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,e,s)=>{const i=s?.renderBefore??e;let r=i._$litPart$;if(void 0===r){const t=s?.renderBefore??null;i._$litPart$=r=new X(e.insertBefore(O(),t),t,void 0,s??{})}return r._$AI(t),r})(e,this.renderRoot,this.renderOptions)}connectedCallback(){super.connectedCallback(),this._$Do?.setConnected(!0)}disconnectedCallback(){super.disconnectedCallback(),this._$Do?.setConnected(!1)}render(){return B}}nt._$litElement$=!0,nt.finalized=!0,at.litElementHydrateSupport?.({LitElement:nt});const dt=at.litElementPolyfillSupport;dt?.({LitElement:nt}),(at.litElementVersions??=[]).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const ht=t=>(e,s)=>{void 0!==s?s.addInitializer(()=>{customElements.define(t,e)}):customElements.define(t,e)},lt={attribute:!0,type:String,converter:$,reflect:!1,hasChanged:m},ct=(t=lt,e,s)=>{const{kind:i,metadata:r}=s;let o=globalThis.litPropertyMetadata.get(r);if(void 0===o&&globalThis.litPropertyMetadata.set(r,o=new Map),"setter"===i&&((t=Object.create(t)).wrapped=!0),o.set(s.name,t),"accessor"===i){const{name:i}=s;return{set(s){const r=e.get.call(this);e.set.call(this,s),this.requestUpdate(i,r,t,!0,s)},init(e){return void 0!==e&&this.C(i,void 0,t,e),e}}}if("setter"===i){const{name:i}=s;return function(s){const r=this[i];e.call(this,s),this.requestUpdate(i,r,t,!0,s)}}throw Error("Unsupported decorator location: "+i)};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function pt(t){return(e,s)=>"object"==typeof s?ct(t,e,s):((t,e,s)=>{const i=e.hasOwnProperty(s);return e.constructor.createProperty(s,t),i?Object.getOwnPropertyDescriptor(e,s):void 0})(t,e,s)}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function ut(t){return pt({...t,state:!0,attribute:!1})}const _t=a`
  :host {
    display: block;
    padding: 16px;
    --mdc-theme-primary: var(--primary-color);
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .header h1 {
    margin: 0;
    font-size: 1.5em;
    font-weight: 400;
    color: var(--primary-text-color);
  }

  .tabs {
    display: flex;
    border-bottom: 1px solid var(--divider-color);
    margin-bottom: 16px;
  }

  .tab {
    padding: 12px 20px;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    color: var(--secondary-text-color);
    font-size: 0.95em;
    font-weight: 500;
    transition: color 0.2s, border-color 0.2s;
    background: none;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .tab:hover {
    color: var(--primary-text-color);
  }

  .tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
  }
`,vt=a`
  .card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
    margin-bottom: 16px;
  }

  .card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1em;
    font-weight: 500;
    color: var(--primary-text-color);
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 12px;
  }

  .stat-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }

  .stat-label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }

  .stat-value {
    font-size: 1.3em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
`,gt=a`
  .table-controls {
    display: flex;
    gap: 12px;
    align-items: flex-end;
    flex-wrap: wrap;
    margin-bottom: 16px;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .input-group label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
  }

  .input-group input {
    padding: 8px 12px;
    border: 1px solid var(--divider-color);
    border-radius: 6px;
    background: var(--card-background-color, white);
    color: var(--primary-text-color);
    font-size: 0.9em;
  }

  button.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.9em;
    font-weight: 500;
  }

  button.btn:hover {
    opacity: 0.9;
  }

  button.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }

  th {
    text-align: left;
    padding: 8px 6px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
    white-space: nowrap;
  }

  td {
    padding: 6px;
    border-bottom: 1px solid var(--divider-color);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-top: 12px;
    font-size: 0.9em;
    color: var(--secondary-text-color);
  }

  .summary-row {
    background: var(--primary-background-color);
    font-weight: 600;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`;let yt=class extends nt{constructor(){super(...arguments),this.entryId="",this._data=null,this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._data&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{this._data=await this.hass.callWS({type:"my_solar_cells/get_overview",entry_id:this.entryId})}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}render(){if(this._loading)return W`<div class="loading">Loading overview...</div>`;if(this._error)return W`<div class="no-data">Error: ${this._error}</div>`;if(!this._data)return W`<div class="no-data">No data available</div>`;const t=this._data;return W`
      <div class="card">
        <h3>Database Summary</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">Last Tibber Sync</div>
            <div class="stat-value">${t.last_tibber_sync?this._formatTimestamp(t.last_tibber_sync):"Never"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Hourly Records</div>
            <div class="stat-value">${t.hourly_record_count.toLocaleString()}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Spot Prices</div>
            <div class="stat-value">${t.spot_price_count.toLocaleString()}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">First Record</div>
            <div class="stat-value">${t.first_timestamp?this._formatDate(t.first_timestamp):"N/A"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Last Record</div>
            <div class="stat-value">${t.last_timestamp?this._formatDate(t.last_timestamp):"N/A"}</div>
          </div>
        </div>
      </div>

      ${this._renderYearlyParams(t.yearly_params)}
    `}_renderYearlyParams(t){const e=Object.keys(t).sort();return 0===e.length?q:W`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Year</th>
                <th>Tax Reduction</th>
                <th>Grid Comp.</th>
                <th>Transfer Fee</th>
                <th>Energy Tax</th>
                <th>Installed kW</th>
              </tr>
            </thead>
            <tbody>
              ${e.map(e=>{const s=t[e];return W`
                  <tr>
                    <td>${e}</td>
                    <td>${this._fmt(s.tax_reduction)}</td>
                    <td>${this._fmt(s.grid_compensation)}</td>
                    <td>${this._fmt(s.transfer_fee)}</td>
                    <td>${this._fmt(s.energy_tax)}</td>
                    <td>${null!=s.installed_kw?s.installed_kw:"-"}</td>
                  </tr>
                `})}
            </tbody>
          </table>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_formatTimestamp(t){try{return new Date(t).toLocaleString("sv-SE")}catch{return t}}_formatDate(t){try{return t.substring(0,10)}catch{return t}}};yt.styles=[vt,gt],t([pt({attribute:!1})],yt.prototype,"hass",void 0),t([pt()],yt.prototype,"entryId",void 0),t([ut()],yt.prototype,"_data",void 0),t([ut()],yt.prototype,"_loading",void 0),t([ut()],yt.prototype,"_error",void 0),yt=t([ht("overview-view")],yt);const ft=50;let $t=class extends nt{constructor(){super(...arguments),this.entryId="",this._startDate="",this._endDate="",this._records=[],this._totalCount=0,this._offset=0,this._loading=!1,this._error=""}connectedCallback(){super.connectedCallback();const t=(new Date).toISOString().substring(0,10);this._startDate=t,this._endDate=t}render(){return W`
      <div class="card">
        <h3>Hourly Energy Records</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Start Date</label>
            <input
              type="date"
              .value=${this._startDate}
              @change=${t=>{this._startDate=t.target.value}}
            />
          </div>
          <div class="input-group">
            <label>End Date</label>
            <input
              type="date"
              .value=${this._endDate}
              @change=${t=>{this._endDate=t.target.value}}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading?"Loading...":"Load"}
          </button>
        </div>

        ${this._error?W`<div class="no-data">Error: ${this._error}</div>`:""}
        ${this._records.length>0?this._renderTable():""}
        ${this._loading||0!==this._records.length||this._error?"":W`<div class="no-data">
              Select a date range and click Load
            </div>`}
      </div>
    `}_renderTable(){const t=Math.ceil(this._totalCount/ft),e=Math.floor(this._offset/ft)+1;return W`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Purchased kWh</th>
              <th>Cost SEK</th>
              <th>Sold kWh</th>
              <th>Profit SEK</th>
              <th>Own Use kWh</th>
              <th>Saved SEK</th>
              <th>Price Level</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(t=>W`
                <tr>
                  <td>${this._formatTs(t.timestamp)}</td>
                  <td>${t.purchased.toFixed(3)}</td>
                  <td>${t.purchased_cost.toFixed(2)}</td>
                  <td>${t.production_sold.toFixed(3)}</td>
                  <td>${t.production_sold_profit.toFixed(2)}</td>
                  <td>${t.production_own_use.toFixed(3)}</td>
                  <td>${t.production_own_use_profit.toFixed(2)}</td>
                  <td>${t.price_level||"-"}</td>
                </tr>
              `)}
          </tbody>
        </table>
      </div>
      <div class="pagination">
        <button
          class="btn"
          ?disabled=${0===this._offset||this._loading}
          @click=${this._prevPage}
        >
          Prev
        </button>
        <span>Page ${e} of ${t} (${this._totalCount} records)</span>
        <button
          class="btn"
          ?disabled=${this._offset+ft>=this._totalCount||this._loading}
          @click=${this._nextPage}
        >
          Next
        </button>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._startDate&&this._endDate){this._loading=!0,this._error="";try{const t=`${this._startDate}T00:00:00`,e=new Date(this._endDate);e.setDate(e.getDate()+1);const s=`${e.toISOString().substring(0,10)}T00:00:00`,i=await this.hass.callWS({type:"my_solar_cells/get_hourly_energy",entry_id:this.entryId,start_date:t,end_date:s,offset:this._offset,limit:ft});this._records=i.records,this._totalCount=i.total_count}catch(t){this._error=t.message||"Failed to fetch data",this._records=[],this._totalCount=0}this._loading=!1}}_prevPage(){this._offset=Math.max(0,this._offset-ft),this._fetch()}_nextPage(){this._offset+=ft,this._fetch()}_formatTs(t){try{return t.replace("T"," ").substring(0,19)}catch{return t}}};$t.styles=[vt,gt],t([pt({attribute:!1})],$t.prototype,"hass",void 0),t([pt()],$t.prototype,"entryId",void 0),t([ut()],$t.prototype,"_startDate",void 0),t([ut()],$t.prototype,"_endDate",void 0),t([ut()],$t.prototype,"_records",void 0),t([ut()],$t.prototype,"_totalCount",void 0),t([ut()],$t.prototype,"_offset",void 0),t([ut()],$t.prototype,"_loading",void 0),t([ut()],$t.prototype,"_error",void 0),$t=t([ht("hourly-energy-view")],$t);let mt=class extends nt{constructor(){super(...arguments),this.entryId="",this._date="",this._prices=[],this._loading=!1,this._error="",this._fetched=!1}connectedCallback(){super.connectedCallback(),this._date=(new Date).toISOString().substring(0,10)}render(){return W`
      <div class="card">
        <h3>Spot Prices</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Date</label>
            <input
              type="date"
              .value=${this._date}
              @change=${t=>{this._date=t.target.value}}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading?"Loading...":"Load"}
          </button>
        </div>

        ${this._error?W`<div class="no-data">Error: ${this._error}</div>`:""}
        ${this._prices.length>0?this._renderTable():""}
        ${this._fetched&&0===this._prices.length&&!this._error?W`<div class="no-data">No prices found for ${this._date}</div>`:""}
        ${this._fetched||this._error?"":W`<div class="no-data">Select a date and click Load</div>`}
      </div>
    `}_renderTable(){const t=this._prices.map(t=>t.total),e=t.reduce((t,e)=>t+e,0)/t.length,s=Math.min(...t),i=Math.max(...t);return W`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>Total SEK/kWh</th>
              <th>Energy</th>
              <th>Tax</th>
              <th>Level</th>
            </tr>
          </thead>
          <tbody>
            ${this._prices.map(t=>W`
                <tr>
                  <td>${this._formatTime(t.timestamp)}</td>
                  <td>${t.total.toFixed(4)}</td>
                  <td>${t.energy.toFixed(4)}</td>
                  <td>${t.tax.toFixed(4)}</td>
                  <td>${t.level||"-"}</td>
                </tr>
              `)}
            <tr class="summary-row">
              <td>Summary</td>
              <td>
                Avg: ${e.toFixed(4)} / Min: ${s.toFixed(4)} / Max:
                ${i.toFixed(4)}
              </td>
              <td></td>
              <td></td>
              <td>${this._prices.length} entries</td>
            </tr>
          </tbody>
        </table>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._date){this._loading=!0,this._error="",this._fetched=!1;try{const t=await this.hass.callWS({type:"my_solar_cells/get_spot_prices",entry_id:this.entryId,date:this._date});this._prices=t.prices}catch(t){this._error=t.message||"Failed to fetch data",this._prices=[]}this._fetched=!0,this._loading=!1}}_formatTime(t){try{return(t.includes("T")?t.split("T")[1]:t).substring(0,5)}catch{return t}}};mt.styles=[vt,gt],t([pt({attribute:!1})],mt.prototype,"hass",void 0),t([pt()],mt.prototype,"entryId",void 0),t([ut()],mt.prototype,"_date",void 0),t([ut()],mt.prototype,"_prices",void 0),t([ut()],mt.prototype,"_loading",void 0),t([ut()],mt.prototype,"_error",void 0),t([ut()],mt.prototype,"_fetched",void 0),mt=t([ht("spot-prices-view")],mt);let bt=class extends nt{constructor(){super(...arguments),this.entryId="",this._sensors=[],this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._sensors.length&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_sensor_config",entry_id:this.entryId});this._sensors=t.sensors}catch(t){this._error=t.message||"Failed to fetch sensor config"}this._loading=!1}}render(){if(this._loading)return W`<div class="loading">Loading sensor configuration...</div>`;if(this._error)return W`<div class="no-data">Error: ${this._error}</div>`;this._sensors.filter(t=>t.entity_id);const t=this._sensors.filter(t=>!t.entity_id);return W`
      <div class="info-box">
        These HA sensors are used to enrich Tibber data with production and
        battery information. <strong>production_own_use</strong> is calculated
        as: total production minus grid export. Both sensors must be configured
        for this to work. Sensors are configured in the integration setup flow.
      </div>

      <div class="card">
        <h3>Sensor Configuration</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Status</th>
                <th>Role</th>
                <th>Description</th>
                <th>Entity ID</th>
                <th>Current State</th>
                <th>Last Stored Reading</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>

      ${t.length>0?W`
            <div class="card">
              <h3>Missing Sensors</h3>
              <p style="color: var(--secondary-text-color); font-size: 0.9em;">
                The following sensors are not configured. To calculate
                <strong>production_own_use</strong>, you need at least
                <strong>production</strong> configured. For best results,
                configure both <strong>production</strong> and
                <strong>grid_export</strong>.
              </p>
              <ul style="color: var(--secondary-text-color); font-size: 0.9em;">
                ${t.map(t=>W`<li>${t.description} (<code>${t.role}</code>)</li>`)}
              </ul>
            </div>
          `:q}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          Refresh
        </button>
      </div>
    `}_renderRow(t){const e=!!t.entity_id;return W`
      <tr>
        <td>
          <span
            class="status-dot ${e?"configured":"missing"}"
          ></span>
        </td>
        <td><strong>${t.role}</strong></td>
        <td>${t.description}</td>
        <td>
          ${e?W`<span class="entity-id">${t.entity_id}</span>`:W`<span class="not-configured">Not configured</span>`}
        </td>
        <td>
          ${null!=t.current_state?t.current_state:W`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${null!=t.last_stored_reading?t.last_stored_reading.toFixed(3):W`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `}};bt.styles=[vt,gt,a`
      .status-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 50%;
        margin-right: 8px;
      }
      .status-dot.configured {
        background: var(--success-color, #4caf50);
      }
      .status-dot.missing {
        background: var(--error-color, #f44336);
      }
      .entity-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--primary-text-color);
      }
      .not-configured {
        color: var(--secondary-text-color);
        font-style: italic;
      }
      .info-box {
        background: var(--primary-background-color);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.9em;
        color: var(--secondary-text-color);
        line-height: 1.5;
      }
    `],t([pt({attribute:!1})],bt.prototype,"hass",void 0),t([pt()],bt.prototype,"entryId",void 0),t([ut()],bt.prototype,"_sensors",void 0),t([ut()],bt.prototype,"_loading",void 0),t([ut()],bt.prototype,"_error",void 0),bt=t([ht("sensors-view")],bt);let xt=class extends nt{constructor(){super(...arguments),this._activeTab="overview"}get _entryId(){return this.panel?.config?.entry_id||""}render(){return W`
      <div class="content">
        <div class="header">
          <h1>Solar Data Browser</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${"overview"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="overview"}
          >
            Overview
          </button>
          <button
            class="tab ${"hourly"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="hourly"}
          >
            Hourly Energy
          </button>
          <button
            class="tab ${"spot"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="spot"}
          >
            Spot Prices
          </button>
          <button
            class="tab ${"sensors"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="sensors"}
          >
            Sensors
          </button>
        </div>
        ${this._renderActiveTab()}
      </div>
    `}_renderActiveTab(){switch(this._activeTab){case"overview":return W`
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        `;case"hourly":return W`
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        `;case"spot":return W`
          <spot-prices-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></spot-prices-view>
        `;case"sensors":return W`
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        `}}};xt.styles=[_t,a`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }
    `],t([pt({attribute:!1})],xt.prototype,"hass",void 0),t([pt({attribute:!1})],xt.prototype,"narrow",void 0),t([pt({attribute:!1})],xt.prototype,"route",void 0),t([pt({attribute:!1})],xt.prototype,"panel",void 0),t([ut()],xt.prototype,"_activeTab",void 0),xt=t([ht("my-solar-cells-panel")],xt);export{xt as MySolarCellsPanel};
