function t(t,e,s,r){var i,o=arguments.length,a=o<3?e:null===r?r=Object.getOwnPropertyDescriptor(e,s):r;if("object"==typeof Reflect&&"function"==typeof Reflect.decorate)a=Reflect.decorate(t,e,s,r);else for(var n=t.length-1;n>=0;n--)(i=t[n])&&(a=(o<3?i(a):o>3?i(e,s,a):i(e,s))||a);return o>3&&a&&Object.defineProperty(e,s,a),a}"function"==typeof SuppressedError&&SuppressedError;
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const e=globalThis,s=e.ShadowRoot&&(void 0===e.ShadyCSS||e.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,r=Symbol(),i=new WeakMap;let o=class{constructor(t,e,s){if(this._$cssResult$=!0,s!==r)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(s&&void 0===t){const s=void 0!==e&&1===e.length;s&&(t=i.get(e)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),s&&i.set(e,t))}return t}toString(){return this.cssText}};const a=(t,...e)=>{const s=1===t.length?t[0]:e.reduce((e,s,r)=>e+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+t[r+1],t[0]);return new o(s,t,r)},n=s?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let e="";for(const s of t.cssRules)e+=s.cssText;return(t=>new o("string"==typeof t?t:t+"",void 0,r))(e)})(t):t,{is:d,defineProperty:l,getOwnPropertyDescriptor:h,getOwnPropertyNames:c,getOwnPropertySymbols:p,getPrototypeOf:u}=Object,_=globalThis,g=_.trustedTypes,v=g?g.emptyScript:"",f=_.reactiveElementPolyfillSupport,y=(t,e)=>t,m={toAttribute(t,e){switch(e){case Boolean:t=t?v:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,e){let s=t;switch(e){case Boolean:s=null!==t;break;case Number:s=null===t?null:Number(t);break;case Object:case Array:try{s=JSON.parse(t)}catch(t){s=null}}return s}},b=(t,e)=>!d(t,e),$={attribute:!0,type:String,converter:m,reflect:!1,useDefault:!1,hasChanged:b};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */Symbol.metadata??=Symbol("metadata"),_.litPropertyMetadata??=new WeakMap;let x=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??=[]).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=$){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const s=Symbol(),r=this.getPropertyDescriptor(t,s,e);void 0!==r&&l(this.prototype,t,r)}}static getPropertyDescriptor(t,e,s){const{get:r,set:i}=h(this.prototype,t)??{get(){return this[e]},set(t){this[e]=t}};return{get:r,set(e){const o=r?.call(this);i?.call(this,e),this.requestUpdate(t,o,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??$}static _$Ei(){if(this.hasOwnProperty(y("elementProperties")))return;const t=u(this);t.finalize(),void 0!==t.l&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(y("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(y("properties"))){const t=this.properties,e=[...c(t),...p(t)];for(const s of e)this.createProperty(s,t[s])}const t=this[Symbol.metadata];if(null!==t){const e=litPropertyMetadata.get(t);if(void 0!==e)for(const[t,s]of e)this.elementProperties.set(t,s)}this._$Eh=new Map;for(const[t,e]of this.elementProperties){const s=this._$Eu(t,e);void 0!==s&&this._$Eh.set(s,t)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const t of s)e.unshift(n(t))}else void 0!==t&&e.push(n(t));return e}static _$Eu(t,e){const s=e.attribute;return!1===s?void 0:"string"==typeof s?s:"string"==typeof t?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){this._$ES=new Promise(t=>this.enableUpdating=t),this._$AL=new Map,this._$E_(),this.requestUpdate(),this.constructor.l?.forEach(t=>t(this))}addController(t){(this._$EO??=new Set).add(t),void 0!==this.renderRoot&&this.isConnected&&t.hostConnected?.()}removeController(t){this._$EO?.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const s of e.keys())this.hasOwnProperty(s)&&(t.set(s,this[s]),delete this[s]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return((t,r)=>{if(s)t.adoptedStyleSheets=r.map(t=>t instanceof CSSStyleSheet?t:t.styleSheet);else for(const s of r){const r=document.createElement("style"),i=e.litNonce;void 0!==i&&r.setAttribute("nonce",i),r.textContent=s.cssText,t.appendChild(r)}})(t,this.constructor.elementStyles),t}connectedCallback(){this.renderRoot??=this.createRenderRoot(),this.enableUpdating(!0),this._$EO?.forEach(t=>t.hostConnected?.())}enableUpdating(t){}disconnectedCallback(){this._$EO?.forEach(t=>t.hostDisconnected?.())}attributeChangedCallback(t,e,s){this._$AK(t,s)}_$ET(t,e){const s=this.constructor.elementProperties.get(t),r=this.constructor._$Eu(t,s);if(void 0!==r&&!0===s.reflect){const i=(void 0!==s.converter?.toAttribute?s.converter:m).toAttribute(e,s.type);this._$Em=t,null==i?this.removeAttribute(r):this.setAttribute(r,i),this._$Em=null}}_$AK(t,e){const s=this.constructor,r=s._$Eh.get(t);if(void 0!==r&&this._$Em!==r){const t=s.getPropertyOptions(r),i="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==t.converter?.fromAttribute?t.converter:m;this._$Em=r;const o=i.fromAttribute(e,t.type);this[r]=o??this._$Ej?.get(r)??o,this._$Em=null}}requestUpdate(t,e,s,r=!1,i){if(void 0!==t){const o=this.constructor;if(!1===r&&(i=this[t]),s??=o.getPropertyOptions(t),!((s.hasChanged??b)(i,e)||s.useDefault&&s.reflect&&i===this._$Ej?.get(t)&&!this.hasAttribute(o._$Eu(t,s))))return;this.C(t,e,s)}!1===this.isUpdatePending&&(this._$ES=this._$EP())}C(t,e,{useDefault:s,reflect:r,wrapped:i},o){s&&!(this._$Ej??=new Map).has(t)&&(this._$Ej.set(t,o??e??this[t]),!0!==i||void 0!==o)||(this._$AL.has(t)||(this.hasUpdated||s||(e=void 0),this._$AL.set(t,e)),!0===r&&this._$Em!==t&&(this._$Eq??=new Set).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??=this.createRenderRoot(),this._$Ep){for(const[t,e]of this._$Ep)this[t]=e;this._$Ep=void 0}const t=this.constructor.elementProperties;if(t.size>0)for(const[e,s]of t){const{wrapped:t}=s,r=this[e];!0!==t||this._$AL.has(e)||void 0===r||this.C(e,void 0,s,r)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),this._$EO?.forEach(t=>t.hostUpdate?.()),this.update(e)):this._$EM()}catch(e){throw t=!1,this._$EM(),e}t&&this._$AE(e)}willUpdate(t){}_$AE(t){this._$EO?.forEach(t=>t.hostUpdated?.()),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&=this._$Eq.forEach(t=>this._$ET(t,this[t])),this._$EM()}updated(t){}firstUpdated(t){}};x.elementStyles=[],x.shadowRootOptions={mode:"open"},x[y("elementProperties")]=new Map,x[y("finalized")]=new Map,f?.({ReactiveElement:x}),(_.reactiveElementVersions??=[]).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const w=globalThis,A=t=>t,S=w.trustedTypes,E=S?S.createPolicy("lit-html",{createHTML:t=>t}):void 0,k="$lit$",P=`lit$${Math.random().toFixed(9).slice(2)}$`,C="?"+P,T=`<${C}>`,D=document,O=()=>D.createComment(""),U=t=>null===t||"object"!=typeof t&&"function"!=typeof t,R=Array.isArray,M="[ \t\n\f\r]",I=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,N=/-->/g,H=/>/g,z=RegExp(`>|${M}(?:([^\\s"'>=/]+)(${M}*=${M}*(?:[^ \t\n\f\r"'\`<>=]|("|')|))|$)`,"g"),L=/'/g,j=/"/g,W=/^(?:script|style|textarea|title)$/i,F=(t=>(e,...s)=>({_$litType$:t,strings:e,values:s}))(1),q=Symbol.for("lit-noChange"),B=Symbol.for("lit-nothing"),V=new WeakMap,Y=D.createTreeWalker(D,129);function K(t,e){if(!R(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return void 0!==E?E.createHTML(e):e}const J=(t,e)=>{const s=t.length-1,r=[];let i,o=2===e?"<svg>":3===e?"<math>":"",a=I;for(let e=0;e<s;e++){const s=t[e];let n,d,l=-1,h=0;for(;h<s.length&&(a.lastIndex=h,d=a.exec(s),null!==d);)h=a.lastIndex,a===I?"!--"===d[1]?a=N:void 0!==d[1]?a=H:void 0!==d[2]?(W.test(d[2])&&(i=RegExp("</"+d[2],"g")),a=z):void 0!==d[3]&&(a=z):a===z?">"===d[0]?(a=i??I,l=-1):void 0===d[1]?l=-2:(l=a.lastIndex-d[2].length,n=d[1],a=void 0===d[3]?z:'"'===d[3]?j:L):a===j||a===L?a=z:a===N||a===H?a=I:(a=z,i=void 0);const c=a===z&&t[e+1].startsWith("/>")?" ":"";o+=a===I?s+T:l>=0?(r.push(n),s.slice(0,l)+k+s.slice(l)+P+c):s+P+(-2===l?e:c)}return[K(t,o+(t[s]||"<?>")+(2===e?"</svg>":3===e?"</math>":"")),r]};class Z{constructor({strings:t,_$litType$:e},s){let r;this.parts=[];let i=0,o=0;const a=t.length-1,n=this.parts,[d,l]=J(t,e);if(this.el=Z.createElement(d,s),Y.currentNode=this.el.content,2===e||3===e){const t=this.el.content.firstChild;t.replaceWith(...t.childNodes)}for(;null!==(r=Y.nextNode())&&n.length<a;){if(1===r.nodeType){if(r.hasAttributes())for(const t of r.getAttributeNames())if(t.endsWith(k)){const e=l[o++],s=r.getAttribute(t).split(P),a=/([.?@])?(.*)/.exec(e);n.push({type:1,index:i,name:a[2],strings:s,ctor:"."===a[1]?et:"?"===a[1]?st:"@"===a[1]?rt:tt}),r.removeAttribute(t)}else t.startsWith(P)&&(n.push({type:6,index:i}),r.removeAttribute(t));if(W.test(r.tagName)){const t=r.textContent.split(P),e=t.length-1;if(e>0){r.textContent=S?S.emptyScript:"";for(let s=0;s<e;s++)r.append(t[s],O()),Y.nextNode(),n.push({type:2,index:++i});r.append(t[e],O())}}}else if(8===r.nodeType)if(r.data===C)n.push({type:2,index:i});else{let t=-1;for(;-1!==(t=r.data.indexOf(P,t+1));)n.push({type:7,index:i}),t+=P.length-1}i++}}static createElement(t,e){const s=D.createElement("template");return s.innerHTML=t,s}}function G(t,e,s=t,r){if(e===q)return e;let i=void 0!==r?s._$Co?.[r]:s._$Cl;const o=U(e)?void 0:e._$litDirective$;return i?.constructor!==o&&(i?._$AO?.(!1),void 0===o?i=void 0:(i=new o(t),i._$AT(t,s,r)),void 0!==r?(s._$Co??=[])[r]=i:s._$Cl=i),void 0!==i&&(e=G(t,i._$AS(t,e.values),i,r)),e}class Q{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:s}=this._$AD,r=(t?.creationScope??D).importNode(e,!0);Y.currentNode=r;let i=Y.nextNode(),o=0,a=0,n=s[0];for(;void 0!==n;){if(o===n.index){let e;2===n.type?e=new X(i,i.nextSibling,this,t):1===n.type?e=new n.ctor(i,n.name,n.strings,this,t):6===n.type&&(e=new it(i,this,t)),this._$AV.push(e),n=s[++a]}o!==n?.index&&(i=Y.nextNode(),o++)}return Y.currentNode=D,r}p(t){let e=0;for(const s of this._$AV)void 0!==s&&(void 0!==s.strings?(s._$AI(t,s,e),e+=s.strings.length-2):s._$AI(t[e])),e++}}class X{get _$AU(){return this._$AM?._$AU??this._$Cv}constructor(t,e,s,r){this.type=2,this._$AH=B,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=s,this.options=r,this._$Cv=r?.isConnected??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return void 0!==e&&11===t?.nodeType&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=G(this,t,e),U(t)?t===B||null==t||""===t?(this._$AH!==B&&this._$AR(),this._$AH=B):t!==this._$AH&&t!==q&&this._(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>R(t)||"function"==typeof t?.[Symbol.iterator])(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==B&&U(this._$AH)?this._$AA.nextSibling.data=t:this.T(D.createTextNode(t)),this._$AH=t}$(t){const{values:e,_$litType$:s}=t,r="number"==typeof s?this._$AC(t):(void 0===s.el&&(s.el=Z.createElement(K(s.h,s.h[0]),this.options)),s);if(this._$AH?._$AD===r)this._$AH.p(e);else{const t=new Q(r,this),s=t.u(this.options);t.p(e),this.T(s),this._$AH=t}}_$AC(t){let e=V.get(t.strings);return void 0===e&&V.set(t.strings,e=new Z(t)),e}k(t){R(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let s,r=0;for(const i of t)r===e.length?e.push(s=new X(this.O(O()),this.O(O()),this,this.options)):s=e[r],s._$AI(i),r++;r<e.length&&(this._$AR(s&&s._$AB.nextSibling,r),e.length=r)}_$AR(t=this._$AA.nextSibling,e){for(this._$AP?.(!1,!0,e);t!==this._$AB;){const e=A(t).nextSibling;A(t).remove(),t=e}}setConnected(t){void 0===this._$AM&&(this._$Cv=t,this._$AP?.(t))}}class tt{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,s,r,i){this.type=1,this._$AH=B,this._$AN=void 0,this.element=t,this.name=e,this._$AM=r,this.options=i,s.length>2||""!==s[0]||""!==s[1]?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=B}_$AI(t,e=this,s,r){const i=this.strings;let o=!1;if(void 0===i)t=G(this,t,e,0),o=!U(t)||t!==this._$AH&&t!==q,o&&(this._$AH=t);else{const r=t;let a,n;for(t=i[0],a=0;a<i.length-1;a++)n=G(this,r[s+a],e,a),n===q&&(n=this._$AH[a]),o||=!U(n)||n!==this._$AH[a],n===B?t=B:t!==B&&(t+=(n??"")+i[a+1]),this._$AH[a]=n}o&&!r&&this.j(t)}j(t){t===B?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class et extends tt{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===B?void 0:t}}class st extends tt{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==B)}}class rt extends tt{constructor(t,e,s,r,i){super(t,e,s,r,i),this.type=5}_$AI(t,e=this){if((t=G(this,t,e,0)??B)===q)return;const s=this._$AH,r=t===B&&s!==B||t.capture!==s.capture||t.once!==s.once||t.passive!==s.passive,i=t!==B&&(s===B||r);r&&this.element.removeEventListener(this.name,this,s),i&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){"function"==typeof this._$AH?this._$AH.call(this.options?.host??this.element,t):this._$AH.handleEvent(t)}}class it{constructor(t,e,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){G(this,t)}}const ot=w.litHtmlPolyfillSupport;ot?.(Z,X),(w.litHtmlVersions??=[]).push("3.3.2");const at=globalThis;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class nt extends x{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){const t=super.createRenderRoot();return this.renderOptions.renderBefore??=t.firstChild,t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,e,s)=>{const r=s?.renderBefore??e;let i=r._$litPart$;if(void 0===i){const t=s?.renderBefore??null;r._$litPart$=i=new X(e.insertBefore(O(),t),t,void 0,s??{})}return i._$AI(t),i})(e,this.renderRoot,this.renderOptions)}connectedCallback(){super.connectedCallback(),this._$Do?.setConnected(!0)}disconnectedCallback(){super.disconnectedCallback(),this._$Do?.setConnected(!1)}render(){return q}}nt._$litElement$=!0,nt.finalized=!0,at.litElementHydrateSupport?.({LitElement:nt});const dt=at.litElementPolyfillSupport;dt?.({LitElement:nt}),(at.litElementVersions??=[]).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const lt=t=>(e,s)=>{void 0!==s?s.addInitializer(()=>{customElements.define(t,e)}):customElements.define(t,e)},ht={attribute:!0,type:String,converter:m,reflect:!1,hasChanged:b},ct=(t=ht,e,s)=>{const{kind:r,metadata:i}=s;let o=globalThis.litPropertyMetadata.get(i);if(void 0===o&&globalThis.litPropertyMetadata.set(i,o=new Map),"setter"===r&&((t=Object.create(t)).wrapped=!0),o.set(s.name,t),"accessor"===r){const{name:r}=s;return{set(s){const i=e.get.call(this);e.set.call(this,s),this.requestUpdate(r,i,t,!0,s)},init(e){return void 0!==e&&this.C(r,void 0,t,e),e}}}if("setter"===r){const{name:r}=s;return function(s){const i=this[r];e.call(this,s),this.requestUpdate(r,i,t,!0,s)}}throw Error("Unsupported decorator location: "+r)};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function pt(t){return(e,s)=>"object"==typeof s?ct(t,e,s):((t,e,s)=>{const r=e.hasOwnProperty(s);return e.constructor.createProperty(s,t),r?Object.getOwnPropertyDescriptor(e,s):void 0})(t,e,s)}
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
`,gt=a`
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
`,vt=a`
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
`,ft=[{key:"own_use_kwh",label:"Own Use",color:"#4285f4",unit:"kWh"},{key:"sold_kwh",label:"Sold",color:"#34a853",unit:"kWh"},{key:"own_use_sek",label:"Own Use",color:"#8ab4f8",unit:"SEK"},{key:"sold_sek",label:"Sold",color:"#f4a742",unit:"SEK"}],yt=[{key:"today",label:"Today"},{key:"this_week",label:"This Week"},{key:"this_month",label:"This Month"},{key:"this_year",label:"This Year"}];let mt=class extends nt{render(){if(!this.data)return B;const t=this._getMax("kwh"),e=this._getMax("sek");return F`
      <div class="chart-container">
        ${yt.map(s=>this._renderPeriod(s,this.data[s.key],t,e))}
      </div>
      <div class="legend">
        ${ft.map(t=>F`
            <div class="legend-item">
              <div class="legend-swatch" style="background:${t.color}"></div>
              ${t.label} (${t.unit})
            </div>
          `)}
      </div>
    `}_renderPeriod(t,e,s,r){return F`
      <div class="period-column">
        <div class="period-label">${t.label}</div>
        <div class="bars">
          ${ft.map(t=>{const i=e[t.key],o="kWh"===t.unit?s:r;return F`
              <div class="bar-wrapper">
                <div
                  class="bar"
                  style="height:${o>0?i/o*100:0}%;background:${t.color}"
                ></div>
                <div class="bar-value">${this._formatValue(i,t.unit)}</div>
              </div>
            `})}
        </div>
      </div>
    `}_getMax(t){if(!this.data)return 0;const e="kwh"===t?["own_use_kwh","sold_kwh"]:["own_use_sek","sold_sek"];let s=0;for(const t of yt){const r=this.data[t.key];for(const t of e)r[t]>s&&(s=r[t])}return s}_formatValue(t,e){return t>=1e3?`${(t/1e3).toFixed(1)}k`:t>=10?t.toFixed(0):t.toFixed(1)}};mt.styles=a`
    :host {
      display: block;
    }

    .chart-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    @media (max-width: 600px) {
      .chart-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .period-column {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .period-label {
      font-size: 0.85em;
      font-weight: 500;
      color: var(--primary-text-color);
      margin-bottom: 8px;
    }

    .bars {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 120px;
      width: 100%;
      justify-content: center;
    }

    .bar-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      max-width: 36px;
      height: 100%;
      justify-content: flex-end;
    }

    .bar {
      width: 100%;
      border-radius: 3px 3px 0 0;
      min-height: 2px;
      transition: height 0.3s ease;
    }

    .bar-value {
      font-size: 0.65em;
      color: var(--secondary-text-color);
      margin-top: 4px;
      white-space: nowrap;
      text-align: center;
    }

    .legend {
      display: flex;
      justify-content: center;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75em;
      color: var(--secondary-text-color);
    }

    .legend-swatch {
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }
  `,t([pt({attribute:!1})],mt.prototype,"data",void 0),mt=t([lt("period-summary-chart")],mt);let bt=class extends nt{constructor(){super(...arguments),this.entryId="",this._data=null,this._periodData=null,this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._data&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const[t,e]=await Promise.all([this.hass.callWS({type:"my_solar_cells/get_overview",entry_id:this.entryId}),this.hass.callWS({type:"my_solar_cells/get_period_summaries",entry_id:this.entryId})]);this._data=t,this._periodData=e}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}render(){if(this._loading)return F`<div class="loading">Loading overview...</div>`;if(this._error)return F`<div class="no-data">Error: ${this._error}</div>`;if(!this._data)return F`<div class="no-data">No data available</div>`;const t=this._data;return F`
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
            <div class="stat-label">First Record</div>
            <div class="stat-value">${t.first_timestamp?this._formatDate(t.first_timestamp):"N/A"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Last Record</div>
            <div class="stat-value">${t.last_timestamp?this._formatDate(t.last_timestamp):"N/A"}</div>
          </div>
        </div>
      </div>

      ${this._periodData?F`
            <div class="card">
              <h3>Energy Summary</h3>
              <period-summary-chart .data=${this._periodData}></period-summary-chart>
            </div>
          `:B}

      ${this._renderYearlyParams(t.yearly_params)}
    `}_renderYearlyParams(t){const e=Object.keys(t).sort();return 0===e.length?B:F`
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
              ${e.map(e=>{const s=t[e];return F`
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
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_formatTimestamp(t){try{return new Date(t).toLocaleString("sv-SE")}catch{return t}}_formatDate(t){try{return t.substring(0,10)}catch{return t}}};bt.styles=[gt,vt],t([pt({attribute:!1})],bt.prototype,"hass",void 0),t([pt()],bt.prototype,"entryId",void 0),t([ut()],bt.prototype,"_data",void 0),t([ut()],bt.prototype,"_periodData",void 0),t([ut()],bt.prototype,"_loading",void 0),t([ut()],bt.prototype,"_error",void 0),bt=t([lt("overview-view")],bt);const $t=50;let xt=class extends nt{constructor(){super(...arguments),this.entryId="",this._startDate="",this._endDate="",this._records=[],this._totalCount=0,this._offset=0,this._loading=!1,this._error=""}connectedCallback(){super.connectedCallback();const t=(new Date).toISOString().substring(0,10);this._startDate=t,this._endDate=t}render(){return F`
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

        ${this._error?F`<div class="no-data">Error: ${this._error}</div>`:""}
        ${this._records.length>0?this._renderTable():""}
        ${this._loading||0!==this._records.length||this._error?"":F`<div class="no-data">
              Select a date range and click Load
            </div>`}
      </div>
    `}_renderTable(){const t=Math.ceil(this._totalCount/$t),e=Math.floor(this._offset/$t)+1;return F`
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
              <th>Synced</th>
              <th>Enriched</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(t=>F`
                <tr>
                  <td>${this._formatTs(t.timestamp)}</td>
                  <td>${t.purchased.toFixed(3)}</td>
                  <td>${t.purchased_cost.toFixed(2)}</td>
                  <td>${t.production_sold.toFixed(3)}</td>
                  <td>${t.production_sold_profit.toFixed(2)}</td>
                  <td>${t.production_own_use.toFixed(3)}</td>
                  <td>${t.production_own_use_profit.toFixed(2)}</td>
                  <td>${t.price_level||"-"}</td>
                  <td>${t.synced?"Yes":"No"}</td>
                  <td>${t.sensor_enriched?"Yes":"No"}</td>
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
          ?disabled=${this._offset+$t>=this._totalCount||this._loading}
          @click=${this._nextPage}
        >
          Next
        </button>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._startDate&&this._endDate){this._loading=!0,this._error="";try{const t=`${this._startDate}T00:00:00`,e=new Date(this._endDate);e.setDate(e.getDate()+1);const s=`${e.toISOString().substring(0,10)}T00:00:00`,r=await this.hass.callWS({type:"my_solar_cells/get_hourly_energy",entry_id:this.entryId,start_date:t,end_date:s,offset:this._offset,limit:$t});this._records=r.records,this._totalCount=r.total_count}catch(t){this._error=t.message||"Failed to fetch data",this._records=[],this._totalCount=0}this._loading=!1}}_prevPage(){this._offset=Math.max(0,this._offset-$t),this._fetch()}_nextPage(){this._offset+=$t,this._fetch()}_formatTs(t){try{return t.replace("T"," ").substring(0,19)}catch{return t}}};xt.styles=[gt,vt],t([pt({attribute:!1})],xt.prototype,"hass",void 0),t([pt()],xt.prototype,"entryId",void 0),t([ut()],xt.prototype,"_startDate",void 0),t([ut()],xt.prototype,"_endDate",void 0),t([ut()],xt.prototype,"_records",void 0),t([ut()],xt.prototype,"_totalCount",void 0),t([ut()],xt.prototype,"_offset",void 0),t([ut()],xt.prototype,"_loading",void 0),t([ut()],xt.prototype,"_error",void 0),xt=t([lt("hourly-energy-view")],xt);let wt=class extends nt{constructor(){super(...arguments),this.entryId="",this._sensors=[],this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._sensors.length&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_sensor_config",entry_id:this.entryId});this._sensors=t.sensors}catch(t){this._error=t.message||"Failed to fetch sensor config"}this._loading=!1}}render(){if(this._loading)return F`<div class="loading">Loading sensor configuration...</div>`;if(this._error)return F`<div class="no-data">Error: ${this._error}</div>`;this._sensors.filter(t=>t.entity_id);const t=this._sensors.filter(t=>!t.entity_id);return F`
      <div class="info-box">
        Only the <strong>production</strong> sensor is required — it is used to
        calculate <strong>production_own_use</strong> (total production minus
        grid export). All other data (grid export, grid import) comes from the
        <strong>Tibber API</strong> by default. You can optionally override
        grid export/import with HA sensors for higher accuracy, and add battery
        sensors if you have a battery. Sensors are configured in the
        integration setup flow.
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

      ${t.some(t=>"production"===t.role)?F`
            <div class="card">
              <h3>Required Sensor Missing</h3>
              <p style="color: var(--error-color, #f44336); font-size: 0.9em;">
                The <strong>production</strong> sensor is not configured.
                Without it, <strong>production_own_use</strong> cannot be
                calculated. Please configure it in the integration setup flow.
              </p>
            </div>
          `:B}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          Refresh
        </button>
      </div>
    `}_isRequired(t){return"production"===t}_getFallbackLabel(t){return"grid_export"===t||"grid_import"===t?"Using Tibber API":"Not configured"}_renderRow(t){const e=!!t.entity_id,s=this._isRequired(t.role);return F`
      <tr>
        <td>
          <span class="status-dot ${e?"configured":s?"missing":"optional"}"></span>
        </td>
        <td>
          <strong>${t.role}</strong>
          ${s?F`<span class="required-badge">Required</span>`:F`<span class="optional-badge">Optional</span>`}
        </td>
        <td>${t.description}</td>
        <td>
          ${e?F`<span class="entity-id">${t.entity_id}</span>`:F`<span class="fallback-label">${this._getFallbackLabel(t.role)}</span>`}
        </td>
        <td>
          ${null!=t.current_state?t.current_state:F`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${null!=t.last_stored_reading?t.last_stored_reading.toFixed(3):F`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `}};wt.styles=[gt,vt,a`
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
      .status-dot.optional {
        background: var(--warning-color, #ff9800);
      }
      .fallback-label {
        color: var(--secondary-text-color);
        font-size: 0.85em;
        font-style: italic;
      }
      .required-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--error-color, #f44336);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .optional-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--secondary-text-color);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
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
    `],t([pt({attribute:!1})],wt.prototype,"hass",void 0),t([pt()],wt.prototype,"entryId",void 0),t([ut()],wt.prototype,"_sensors",void 0),t([ut()],wt.prototype,"_loading",void 0),t([ut()],wt.prototype,"_error",void 0),wt=t([lt("sensors-view")],wt);let At=class extends nt{constructor(){super(...arguments),this._activeTab="overview"}get _entryId(){return this.panel?.config?.entry_id||""}render(){return F`
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
            class="tab ${"sensors"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="sensors"}
          >
            Sensors
          </button>
        </div>
        ${this._renderActiveTab()}
      </div>
    `}_renderActiveTab(){switch(this._activeTab){case"overview":return F`
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        `;case"hourly":return F`
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        `;case"sensors":return F`
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        `}}};At.styles=[_t,a`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }
    `],t([pt({attribute:!1})],At.prototype,"hass",void 0),t([pt({attribute:!1})],At.prototype,"narrow",void 0),t([pt({attribute:!1})],At.prototype,"route",void 0),t([pt({attribute:!1})],At.prototype,"panel",void 0),t([ut()],At.prototype,"_activeTab",void 0),At=t([lt("my-solar-cells-panel")],At);export{At as MySolarCellsPanel};
